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
                // 1. Validation cơ bản cho phân trang
                if (pageNumber <= 0 || pageSize <= 0)
                {
                    response.IsSucess = false;
                    response.BusinessCode = BusinessCode.VALIDATION_ERROR;
                    response.Message = "PageNumber và PageSize phải lớn hơn 0.";
                    return response;
                }

                // 2. Chuẩn bị Query và Lấy tổng số lượng (Thực hiện trên Database)
                var baseQuery = _reviewFeeRepository.AsQueryable();

                // Lấy tổng số lượng (KHÔNG PHÂN TRANG)
                var totalItems = await baseQuery.CountAsync();

                if (totalItems == 0)
                {
                    response.IsSucess = true;
                    response.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    response.Message = "Không tìm thấy gói Review Fee nào.";
                    response.Data = new
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalItems = 0,
                        Items = packagesDto
                    };
                    return response;
                }

                // 3. Áp dụng Sắp xếp (Sorting) và Phân trang (Paging) trên Database
                var pagedQuery = baseQuery
                    .OrderByDescending(rf => rf.NumberOfReview) // Sắp xếp theo một trường (ví dụ: Số lượng review)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                // 4. Lấy dữ liệu paged (có Include) từ Database
                // CHỈ LẤY DỮ LIỆU CỦA TRANG HIỆN TẠI VÀ THÔNG TIN LIÊN QUAN
                var pagedReviewFees = await pagedQuery
                    .Include(rf => rf.ReviewFeeDetails)
                    .ToListAsync();

                // 5. Xử lý logic tìm Chính sách giá hiện tại (Trên bộ nhớ)
                var now = DateTime.UtcNow;
                foreach (var reviewFee in pagedReviewFees)
                {
                    var currentDetail = reviewFee.ReviewFeeDetails
                        .Where(d => d.AppliedDate <= now)
                        .OrderByDescending(d => d.AppliedDate)
                        .FirstOrDefault();

                    var packageDto = new ReviewFeePackageResponseDto
                    {
                        ReviewFeeId = reviewFee.ReviewFeeId,
                        NumberOfReview = reviewFee.NumberOfReview,
                        CurrentPricePolicy = currentDetail != null ? new ReviewFeeDetailResponseDto
                        {
                            ReviewFeeDetailId = currentDetail.ReviewFeeDetailId,
                            PricePerReviewFee = currentDetail.PricePerReviewFee,
                            AppliedDate = currentDetail.AppliedDate,
                            PercentOfSystem = currentDetail.PercentOfSystem,
                            PercentOfReviewer = currentDetail.PercentOfReviewer
                        } : null
                    };

                    packagesDto.Add(packageDto);
                }

                // 6. Trả về kết quả phân trang
                response.IsSucess = true;
                response.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                response.Message = "Lấy dữ liệu gói Review Fee thành công.";
                response.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems, // Tổng số lượng tất cả gói
                    Items = packagesDto       // Danh sách gói đã phân trang
                };
            }
            catch (Exception ex)
            {
                response.IsSucess = false;
                response.BusinessCode = BusinessCode.EXCEPTION;
                response.Message = "Lỗi khi lấy dữ liệu gói phí: " + ex.Message;
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
