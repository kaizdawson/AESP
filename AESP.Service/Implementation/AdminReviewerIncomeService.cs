using AESP.API.Helpers;
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



        public async Task<ResponseDTO> GetReviewerDetailAsync(Guid reviewerProfileId, DateTime? fromDate, DateTime? toDate, int pageNumber = 1, int pageSize = 10)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _unitOfWork.GetDbContext();
                var now = DateTimeHelper.NowVN();

                // ========================================
                // 1) Lấy giá review mới nhất
                // ========================================
                var feeDetail = await db.Set<ReviewFeeDetail>()
                    .Where(x => x.AppliedDate <= now)
                    .OrderByDescending(x => x.AppliedDate)
                    .FirstOrDefaultAsync();

                if (feeDetail == null)
                {
                    feeDetail = await db.Set<ReviewFeeDetail>()
                        .OrderBy(x => x.AppliedDate)
                        .FirstOrDefaultAsync();
                }

                if (feeDetail == null)
                {
                    dto.IsSucess = false;
                    dto.BusinessCode = BusinessCode.DATA_NOT_FOUND;
                    dto.Message = "Không tìm thấy cấu hình giá review.";
                    return dto;
                }

                decimal pricePerReview = feeDetail.PricePerReviewFee;
                decimal reviewerPercent = feeDetail.PercentOfReviewer;
                decimal incomePerReview = pricePerReview * reviewerPercent;

                // ========================================
                // 2) Lấy tất cả review liên quan reviewer (KHÔNG CHỈ Completed)
                // ========================================
                var query = db.Set<Review>()
                 .Include(r => r.LearnerAnswer)
                 .ThenInclude(la => la.LearningPathQuestion)
                 .ThenInclude(lpq => lpq.Question)
                 .Include(r => r.LearnerAnswer)
                 .ThenInclude(la => la.LearnerProfile)
                 .ThenInclude(lp => lp.User)
                .Include(r => r.Record)
    .ThenInclude(rec => rec.RecordContent)
        .ThenInclude(rc => rc.LearnerRecord)
            .ThenInclude(lr => lr.LearnerProfile)
                .ThenInclude(lp => lp.User)
                 .Where(r => r.ReviewerProfileId == reviewerProfileId &&
                (r.Status == "Completed"
              || r.Status == "Reported"
              || r.Status == "Reported_Pending"
              || r.Status == "Rejected"))
                .AsQueryable();

                if (fromDate != null)
                    query = query.Where(r => r.CreatedAt >= fromDate);

                if (toDate != null)
                    query = query.Where(r => r.CreatedAt <= toDate);

                var totalItems = await query.CountAsync();



                var reviews = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // ========================================
                // 3) Thống kê tài chính từ TransferTransaction (CHUẨN KẾ TOÁN)
                // ========================================

                var totalEarnedFromSystem = await db.Set<TransferTransaction>()
                    .Where(t => t.ReviewerProfileId == reviewerProfileId
                             && t.Status == "Completed"
                             && t.TransactionType == "ReviewPayment"
                             && (fromDate == null || t.CreatedAt >= fromDate)
                             && (toDate == null || t.CreatedAt <= toDate))
                    .SumAsync(t => (decimal?)t.AmountCoin) ?? 0m;

                var totalPenalty = await db.Set<TransferTransaction>()
                    .Where(t => t.ReviewerProfileId == reviewerProfileId
                             && t.Status == "Completed"
                             && t.TransactionType == "ReviewPenalty"
                             && (fromDate == null || t.CreatedAt >= fromDate)
                             && (toDate == null || t.CreatedAt <= toDate))
                    .SumAsync(t => (decimal?)t.AmountCoin) ?? 0m;

                var totalSpentOnTips = await db.Set<TransferTransaction>()
                    .Where(t => t.ReviewerProfileId == reviewerProfileId
                             && t.Status == "Completed"
                             && t.TransactionType == "ReviewerTip"
                             && (fromDate == null || t.CreatedAt >= fromDate)
                             && (toDate == null || t.CreatedAt <= toDate))
                    .SumAsync(t => (decimal?)t.AmountCoin) ?? 0m;

                var totalCompletedReviews = await query.CountAsync(x => x.Status == "Completed");

                var totalReportedReviews = await query.CountAsync(x =>
                    x.Status == "Reported" || x.Status == "Reported_Pending");

                var totalRejectedReviews = await query.CountAsync(x => x.Status == "Rejected");

                // 🔹 Lấy tiền theo từng review (CHUẨN KẾ TOÁN)
                var reviewPayments = await db.Set<TransferTransaction>()
                    .Where(t =>
                        t.ReviewerProfileId == reviewerProfileId &&
                        t.Status == "Completed" &&
                        t.TransactionType == "ReviewPayment" &&
                        t.ReviewId != null &&
                        (fromDate == null || t.CreatedAt >= fromDate) &&
                        (toDate == null || t.CreatedAt <= toDate))
                    .GroupBy(t => t.ReviewId!.Value)
                    .Select(g => new
                    {
                        ReviewId = g.Key,
                        Amount = g.Sum(x => x.AmountCoin)
                    })
                    .ToDictionaryAsync(x => x.ReviewId, x => x.Amount);





                // ========================================
                // 4) Trả dữ liệu cho Admin
                // ========================================

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết reviewer thành công.";
                dto.Data = new
                {
                    TotalReviews = totalItems,
                    Completed = totalCompletedReviews,
                    Reported = totalReportedReviews,
                    Rejected = totalRejectedReviews,

                    CurrentSystemPricePerReview = pricePerReview,
                    CurrentReviewerIncomePerReview = incomePerReview,

                    TotalEarnedFromSystem = totalEarnedFromSystem,
                    TotalPenalty = totalPenalty,
                    TotalSpentOnTips = totalSpentOnTips,

                    NetIncome = totalEarnedFromSystem - totalPenalty - totalSpentOnTips,

                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),

                    Items = reviews.Select(r => new
                    {
                        r.ReviewId,
                        r.Score,
                        r.Comment,
                        r.Status,
                        ReviewAudioUrl = r.RecordAudioUrl,
                        CreatedAt = r.CreatedAt,

                        Learner = r.LearnerAnswer?.LearnerProfile?.User?.FullName
                            ?? r.Record?.RecordContent?.LearnerRecord?.LearnerProfile?.User?.FullName
                            ?? "Không xác định",


                        Question = r.LearnerAnswer != null
                             && r.LearnerAnswer.LearningPathQuestion != null
                             && r.LearnerAnswer.LearningPathQuestion.Question != null
                            ? r.LearnerAnswer.LearningPathQuestion.Question.Text
                            : r.Record != null
                            ? r.Record.Content
                            : "Không xác định",

                        EarnedFromThisReview =
                            reviewPayments.TryGetValue(r.ReviewId, out var coin)
                                ? coin
                                : 0
                    }).ToList()
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

        public async Task<ResponseDTO> GetReviewerListAsync(
      string? search,
      int pageNumber,
      int pageSize,
      DateTime? fromDate,
      DateTime? toDate)
        {
            var dto = new ResponseDTO();

            try
            {
                var db = _unitOfWork.GetDbContext();

                var query = db.Set<TransferTransaction>()
                    .Include(t => t.ReviewerProfile)
                        .ThenInclude(rp => rp.User)
                    .Where(t => t.Status == "Completed")
                    .Where(t => t.TransactionType == "ReviewPayment")
                    .AsQueryable();

                // ============================
                // 1) Filter theo ngày
                // ============================
                if (fromDate.HasValue)
                {
                    query = query.Where(t => t.CreatedAt >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    // Lấy hết cuối ngày
                    DateTime to = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                    query = query.Where(t => t.CreatedAt <= to);
                }

                // ============================
                // 2) Search theo tên hoặc email
                // ============================
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    query = query.Where(t =>
                        t.ReviewerProfile.User.FullName.ToLower().Contains(keyword) ||
                        t.ReviewerProfile.User.Email.ToLower().Contains(keyword)
                    );
                }

                // ============================
                // 3) Group dữ liệu
                // ============================
                var grouped = query
                    .GroupBy(g => new
                    {
                        g.ReviewerProfileId,
                        g.ReviewerProfile.User.FullName,
                        g.ReviewerProfile.User.Email
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
                     .Where(x => x.Status == "Completed"
                     && x.TransactionType == "ReviewPayment") // ← THÊM DÒNG NÀY!
                     .SumAsync(x => (decimal?)x.AmountCoin) ?? 0m;

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
