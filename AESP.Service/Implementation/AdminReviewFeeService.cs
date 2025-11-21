using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Implementation
{
    public class AdminReviewFeeService : IAdminReviewFeeService
    {
        private readonly IGenericRepository<ReviewFee> _reviewFeeRepository;
        private readonly IGenericRepository<ReviewFeeDetail> _reviewFeeDetailRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminReviewFeeService(IGenericRepository<ReviewFee> reviewFeeRepository, IGenericRepository<ReviewFeeDetail> reviewFeeDetailRepository, IUnitOfWork unitOfWork)
        {
            _reviewFeeRepository = reviewFeeRepository;
            _reviewFeeDetailRepository = reviewFeeDetailRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> CreateReviewFeePackageAndDetailAsync(CreateReviewFeePackageDto dto)
        {
            var response = new ResponseDTO();
            try
            {
                // 🔹 1. VALIDATION
                if (dto.NumberOfReview <= 0 || dto.PricePerReviewFee <= 0 || (dto.PercentOfSystem + dto.PercentOfReviewer) != 1)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.VALIDATION_ERROR;
                    response.Message = "Dữ liệu gói phí không hợp lệ (Số lần Review và Giá gói phải lớn hơn 0, tổng phần trăm chia phải bằng 1).";
                    return response;
                }

                // 🔹 2. TẠO REVIEW FEE (Gói sản phẩm)
                var reviewFee = new ReviewFee
                {
                    ReviewFeeId = Guid.NewGuid(),
                    NumberOfReview = dto.NumberOfReview,
                };

                // ĐÃ SỬA: Thay Add/AddAsync bằng Insert
                await _reviewFeeRepository.Insert(reviewFee);

                // 🔹 3. TẠO REVIEW FEE DETAIL (Định giá và Chính sách)
                var reviewFeeDetail = new ReviewFeeDetail
                {
                    ReviewFeeDetailId = Guid.NewGuid(),
                    ReviewFeeId = reviewFee.ReviewFeeId,
                    PricePerReviewFee = dto.PricePerReviewFee,
                    AppliedDate = DateTime.UtcNow,
                    PercentOfSystem = dto.PercentOfSystem,
                    PercentOfReviewer = dto.PercentOfReviewer
                };

                // ĐÃ SỬA: Thay Add/AddAsync bằng Insert
                await _reviewFeeDetailRepository.Insert(reviewFeeDetail);

                // Lưu thay đổi vào database
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.BusinessCode = BusinessCode.CREATED_SUCCESSFULLY;
                response.Message = "Tạo gói Review Fee và định giá thành công.";
                response.Data = new
                {
                    PackageId = reviewFee.ReviewFeeId,
                    reviewFee.NumberOfReview,
                    PriceDetailId = reviewFeeDetail.ReviewFeeDetailId,
                    reviewFeeDetail.PricePerReviewFee,
                    reviewFeeDetail.PercentOfReviewer
                };
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.EXCEPTION;
                response.Message = "Lỗi khi tạo gói phí: " + ex.Message;
            }
            return response;
        }

        public async Task<ResponseDTO> GetAllReviewFeePackagesAsync(int pageNumber, int pageSize)
        {
            var response = new ResponseDTO();
            var packagesDto = new List<ReviewFeePackageResponseDto>();

            try
            {
                // 1. Validation phân trang
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.VALIDATION_ERROR;
                    response.Message = "PageNumber và PageSize phải lớn hơn 0.";
                    return response;
                }

                var now = DateTime.UtcNow;

                // 2. Lấy tất cả gói + detail, rồi lọc trên DB những gói có ít nhất 1 chính sách đang áp dụng
                var activePackagesQuery = _reviewFeeRepository.AsQueryable()
                    .Include(rf => rf.ReviewFeeDetails)
                    .Where(rf => rf.ReviewFeeDetails.Any(d => d.AppliedDate <= now))
                    .OrderByDescending(rf => rf.NumberOfReview);

                var totalItems = await activePackagesQuery.CountAsync();

                if (totalItems == 0)
                {
                    response.IsSucess = true;
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    response.Message = "Không có gói Review Fee nào đang hoạt động.";
                    response.Data = new
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalItems = 0,
                        Items = packagesDto
                    };
                    return response;
                }

                // 3. Phân trang + lấy dữ liệu
                var pagedPackages = await activePackagesQuery
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 4. Chỉ lấy chính sách giá HIỆN TẠI (mới nhất <= now)
                foreach (var pkg in pagedPackages)
                {
                    var currentDetail = pkg.ReviewFeeDetails
                        .Where(d => d.AppliedDate <= now)
                        .OrderByDescending(d => d.AppliedDate)
                        .FirstOrDefault();

                    // Bắt buộc phải có (do đã filter ở trên)
                    if (currentDetail != null)
                    {
                        packagesDto.Add(new ReviewFeePackageResponseDto
                        {
                            ReviewFeeId = pkg.ReviewFeeId,
                            NumberOfReview = pkg.NumberOfReview,
                            CurrentPricePolicy = new ReviewFeeDetailResponseDto
                            {
                                ReviewFeeDetailId = currentDetail.ReviewFeeDetailId,
                                PricePerReviewFee = currentDetail.PricePerReviewFee,
                                AppliedDate = currentDetail.AppliedDate,
                                PercentOfSystem = currentDetail.PercentOfSystem,
                                PercentOfReviewer = currentDetail.PercentOfReviewer
                            }
                        });
                    }
                }

                response.IsSucess = true;
                response.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                response.Message = "Lấy danh sách gói Review Fee đang hoạt động thành công.";
                response.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Items = packagesDto
                };
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.EXCEPTION;
                response.Message = "Lỗi khi lấy danh sách gói phí: " + ex.Message;
                response.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = 0,
                    Items = packagesDto
                };
            }

            return response;
        }

        public async Task<ResponseDTO> GetReviewFeePackageDetailAsync(Guid reviewFeeId)
        {
            var response = new ResponseDTO();

            try
            {
                var now = DateTime.UtcNow;

                var package = await _reviewFeeRepository.AsQueryable()
                    .Include(rf => rf.ReviewFeeDetails)
                    .FirstOrDefaultAsync(rf => rf.ReviewFeeId == reviewFeeId);

                if (package == null)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    response.Message = "Không tìm thấy gói Review Fee.";
                    return response;
                }

                // Chính sách ĐANG ÁP DỤNG
                var currentPolicy = package.ReviewFeeDetails
                    .Where(d => d.AppliedDate <= now)
                    .OrderByDescending(d => d.AppliedDate)
                    .FirstOrDefault();

                // Chính sách SẮP ÁP DỤNG (nếu có)
                var upcomingPolicy = package.ReviewFeeDetails
                    .Where(d => d.AppliedDate > now)
                    .OrderBy(d => d.AppliedDate)
                    .FirstOrDefault();

                // Tất cả lịch sử chính sách (sắp xếp cũ → mới)
                var historyPolicies = package.ReviewFeeDetails
                    .OrderBy(d => d.AppliedDate)
                    .Select(d => new
                    {
                        d.ReviewFeeDetailId,
                        d.PricePerReviewFee,
                        ReviewerIncome = d.PricePerReviewFee * d.PercentOfReviewer,
                        d.PercentOfReviewer,
                        d.PercentOfSystem,
                        AppliedDate = d.AppliedDate.ToString("dd/MM/yyyy HH:mm"),
                        IsCurrent = d.AppliedDate <= now && (currentPolicy == null || d.ReviewFeeDetailId == currentPolicy.ReviewFeeDetailId),
                        IsUpcoming = upcomingPolicy != null && d.ReviewFeeDetailId == upcomingPolicy.ReviewFeeDetailId
                    })
                    .ToList();

                string status = currentPolicy == null ? "Chưa kích hoạt" :
                                upcomingPolicy == null ? "Đang áp dụng" : "Sắp thay đổi giá";

                response.IsSucess = true;
                response.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                response.Message = "Lấy chi tiết gói Review Fee thành công.";
                response.Data = new
                {
                    ReviewFeeId = package.ReviewFeeId,
                    NumberOfReview = package.NumberOfReview,
                    Status = status,
                    CurrentPolicy = currentPolicy != null ? new
                    {
                        currentPolicy.PricePerReviewFee,
                        ReviewerIncome = currentPolicy.PricePerReviewFee * currentPolicy.PercentOfReviewer,
                        currentPolicy.PercentOfReviewer,
                        AppliedFrom = currentPolicy.AppliedDate.ToString("dd/MM/yyyy HH:mm")
                    } : null,
                    UpcomingPolicy = upcomingPolicy != null ? new
                    {
                        upcomingPolicy.PricePerReviewFee,
                        ReviewerIncome = upcomingPolicy.PricePerReviewFee * upcomingPolicy.PercentOfReviewer,
                        upcomingPolicy.PercentOfReviewer,
                        WillApplyFrom = upcomingPolicy.AppliedDate.ToString("dd/MM/yyyy HH:mm")
                    } : null,
                    HistoryPolicies = historyPolicies
                };
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.EXCEPTION;
                response.Message = "Lỗi khi lấy chi tiết gói phí: " + ex.Message;
            }

            return response;
        }

        public async Task<ResponseDTO> ScheduleNewReviewFeeDetailAsync(UpdateReviewFeeDetailDto dto)
        {
            var response = new ResponseDTO();
            try
            {
                // 🔹 1. VALIDATION CƠ BẢN
                if (dto.PricePerReviewFee <= 0 || (dto.PercentOfSystem + dto.PercentOfReviewer) != 1)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.VALIDATION_ERROR;
                    response.Message = "Giá gói phải lớn hơn 0 và tổng phần trăm chia phải bằng 1.";
                    return response;
                }

                // 🔹 2. KIỂM TRA GÓI REVIEW FEE CÓ TỒN TẠI KHÔNG
                // Không cần tải toàn bộ, chỉ cần kiểm tra sự tồn tại (nếu Repository có phương thức CheckExistsAsync thì dùng, ở đây dùng GetById)
                var existingFee = await _reviewFeeRepository.GetById(dto.ReviewFeeId);
                if (existingFee == null)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    response.Message = "Gói Review Fee không tồn tại.";
                    return response;
                }

                // 🔹 3. TẠO REVIEW FEE DETAIL MỚI (Chính sách giá mới)
                var newReviewFeeDetail = new ReviewFeeDetail
                {
                    ReviewFeeDetailId = Guid.NewGuid(),
                    ReviewFeeId = dto.ReviewFeeId,
                    PricePerReviewFee = dto.PricePerReviewFee,
                    // SỬ DỤNG AppliedDate CỦA ADMIN (Có thể là ngày trong tương lai)
                    AppliedDate = dto.AppliedDate,
                    PercentOfSystem = dto.PercentOfSystem,
                    PercentOfReviewer = dto.PercentOfReviewer
                };

                await _reviewFeeDetailRepository.Insert(newReviewFeeDetail);
                await _unitOfWork.SaveChangeAsync();

                response.IsSucess = true;
                response.BusinessCode = BusinessCode.CREATED_SUCCESSFULLY;
                response.Message = $"Lên lịch chính sách giá mới cho gói {dto.ReviewFeeId} thành công. Chính sách sẽ áp dụng từ {dto.AppliedDate:dd/MM/yyyy}.";
                response.Data = new
                {
                    PriceDetailId = newReviewFeeDetail.ReviewFeeDetailId,
                    newReviewFeeDetail.PricePerReviewFee,
                    newReviewFeeDetail.PercentOfReviewer,
                    newReviewFeeDetail.AppliedDate
                };
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.EXCEPTION;
                response.Message = "Lỗi khi lên lịch chính sách giá: " + ex.Message;
            }
            return response;
        }
    }
}
