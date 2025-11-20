using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.DB;
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
    public class AdminReviewerIncomeService : IAdminReviewerIncomeService
    {
        private readonly IGenericRepository<Review> _reviewRepository;
        private readonly IGenericRepository<ReviewFee> _reviewFeeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminReviewerIncomeService(
            IGenericRepository<Review> reviewRepository,
            IGenericRepository<ReviewFee> reviewFeeRepository,
            IUnitOfWork unitOfWork)
        {
            _reviewRepository = reviewRepository;
            _reviewFeeRepository = reviewFeeRepository;
            _unitOfWork = unitOfWork;
        }



        public async Task<ResponseDTO> GetReviewerDetailAsync(Guid reviewerProfileId, DateTime? fromDate, DateTime? toDate)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _unitOfWork.GetDbContext();

                // ========================================
                // 1) Lấy giá review mới nhất
                // ========================================
                var feeDetail = await db.Set<ReviewFeeDetail>()
                    .OrderByDescending(x => x.AppliedDate)
                    .FirstOrDefaultAsync();

                if (feeDetail == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy cấu hình giá review.";
                    return dto;
                }

                decimal pricePerReview = feeDetail.PricePerReviewFee;
                decimal reviewerPercent = feeDetail.PercentOfReviewer;
                decimal incomePerReview = pricePerReview * reviewerPercent; // ❗ không chia 100 nữa

                // ========================================
                // 2) Lấy tất cả bài review Completed
                // ========================================
                var query = db.Set<Review>()
                    .Include(r => r.LearnerAnswer)
                        .ThenInclude(la => la.LearnerProfile)
                            .ThenInclude(lp => lp.User)
                    .Include(r => r.Record)
                        .ThenInclude(rc => rc.LearnerRecord)
                            .ThenInclude(lr => lr.LearnerProfile)
                                .ThenInclude(lp => lp.User)
                    .Where(r => r.ReviewerProfileId == reviewerProfileId &&
                                r.Status == "Completed")
                    .AsQueryable();

                if (fromDate != null)
                    query = query.Where(r => r.CreatedAt >= fromDate);

                if (toDate != null)
                    query = query.Where(r => r.CreatedAt <= toDate);

                var reviews = await query.ToListAsync();

                // ========================================
                // 3) Mapping data
                // ========================================
                int totalReviews = reviews.Count;
                decimal totalIncome = totalReviews * incomePerReview;

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết reviewer thành công.";
                dto.Data = new
                {
                    TotalReviews = totalReviews,
                    TotalIncome = totalIncome,
                    Items = reviews.Select(r => new
                    {
                        r.ReviewId,
                        r.Score,
                        r.Comment,
                        r.Status,
                        CreatedAt = r.CreatedAt,

                        // ưu tiên lấy từ LearnerAnswer
                        Learner = r.LearnerAnswer?.LearnerProfile?.User?.FullName
                                  ?? r.Record?.LearnerRecord?.LearnerProfile?.User?.FullName
                                  ?? "",

                        Income = incomePerReview
                    })
                };

                return dto;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi: " + ex.Message;
                return dto;
            }
        }

        public async Task<ResponseDTO> GetReviewerListAsync(string? search, int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _unitOfWork.GetDbContext();

                var query = db.Set<TransferTransaction>()
                    .Include(t => t.ReviewerProfile)
                        .ThenInclude(rp => rp.User)
                    .Where(t => t.Status == "Completed")
                    .AsQueryable();

                // Search theo tên hoặc email
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(t =>
                        t.ReviewerProfile.User.FullName.ToLower().Contains(keyword) ||
                        t.ReviewerProfile.User.Email.ToLower().Contains(keyword)
                    );
                }

                var grouped = query
                    .GroupBy(t => new
                    {
                        t.ReviewerProfileId,
                        t.ReviewerProfile.User.FullName,
                        t.ReviewerProfile.User.Email
                    })
                    .Select(g => new
                    {
                        ReviewerProfileId = g.Key.ReviewerProfileId,
                        FullName = g.Key.FullName,
                        Email = g.Key.Email,
                        TotalIncome = g.Sum(x => x.AmountCoin),
                        ReviewCount = g.Count()
                    });

                var total = await grouped.CountAsync();

                var items = await grouped
                    .OrderByDescending(x => x.TotalIncome)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách reviewer thành công.";
                dto.Data = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = total,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy danh sách reviewer: " + ex.Message;
            }

            return dto;
        }

        public async Task<ResponseDTO> GetSummaryAsync()
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _unitOfWork.GetDbContext();

                var totalIncome = await db.Set<TransferTransaction>()
                    .Where(x => x.Status == "Completed")
                    .SumAsync(x => (int?)x.AmountCoin) ?? 0;

                var totalReviews = await db.Set<Review>()
                    .CountAsync();

                var totalReviewer = await db.Set<ReviewerProfile>()
                    .CountAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy thống kê thu nhập reviewer thành công.";
                dto.Data = new
                {
                    TotalIncome = totalIncome,
                    TotalReviews = totalReviews,
                    TotalReviewer = totalReviewer
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy thống kê: " + ex.Message;
            }

            return dto;
        }
    }
}
