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

        public async Task<ResponseDTO> GetReviewerDetailAsync(Guid reviewerId, DateTime? fromDate, DateTime? toDate)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _unitOfWork.GetDbContext();
                var start = fromDate ?? DateTime.MinValue;
                var end = toDate ?? DateTime.UtcNow;

                // Lấy giá review mới nhất
                var pricePerReview = await db.Set<ReviewFee>()
                    .OrderByDescending(x => x.ReviewFeeId)
                    .Select(x => (double?)x.Price)
                    .FirstOrDefaultAsync() ?? 0d;

                // Lấy reviews của reviewer theo khoảng thời gian
                var reviewsQuery = db.Set<Review>()
                    .Include(r => r.Record)
                    .Include(r => r.ReviewerProfile).ThenInclude(rp => rp.User)
                    .Where(r => r.Record != null &&
                                r.Record.CreatedAt >= start &&
                                r.Record.CreatedAt <= end &&
                                r.ReviewerProfileId == reviewerId &&
                                (r.Status == "Completed" || r.Status == "Approved" || r.Status == "Done"));

                var totalReviews = await reviewsQuery.CountAsync();
                var totalIncome = totalReviews * pricePerReview;

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết thu nhập reviewer thành công.";
                dto.Data = new
                {
                    TotalReviews = totalReviews,
                    TotalIncome = totalIncome,
                    PricePerReview = pricePerReview
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

        public async Task<ResponseDTO> GetReviewerListAsync(DateTime? fromDate, DateTime? toDate, string? search, int pageNumber, int pageSize)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _unitOfWork.GetDbContext();
                var start = fromDate ?? DateTime.MinValue;
                var end = toDate ?? DateTime.UtcNow;

                var pricePerReview = await db.Set<ReviewFee>()
                    .OrderByDescending(x => x.ReviewFeeId)
                    .Select(x => (double?)x.Price)
                    .FirstOrDefaultAsync() ?? 0d;

                var baseQuery = db.Set<Review>()
                    .Include(r => r.Record)
                    .Include(r => r.ReviewerProfile).ThenInclude(rp => rp.User)
                    .Where(r => r.Record != null &&
                                r.Record.CreatedAt >= start &&
                                r.Record.CreatedAt <= end &&
                                (r.Status == "Completed" || r.Status == "Approved" || r.Status == "Done"));

                // Search theo tên reviewer hoặc email
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim().ToLower();
                    baseQuery = baseQuery.Where(r =>
                        r.ReviewerProfile.User.FullName.ToLower().Contains(keyword) ||
                        r.ReviewerProfile.User.Email.ToLower().Contains(keyword));
                }

                var grouped = baseQuery
                    .GroupBy(r => new
                    {
                        r.ReviewerProfileId,
                        r.ReviewerProfile.User.FullName,
                        r.ReviewerProfile.User.Email
                    })
                    .Select(g => new
                    {
                        ReviewerId = g.Key.ReviewerProfileId,
                        FullName = g.Key.FullName,
                        Email = g.Key.Email,
                        ReviewCount = g.Count(),
                        Income = g.Count() * pricePerReview
                    });

                var total = await grouped.CountAsync();
                var items = await grouped
                    .OrderByDescending(x => x.ReviewCount)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy danh sách reviewer thành công.";
                dto.Data = new { PageNumber = pageNumber, PageSize = pageSize, TotalItems = total, Items = items };
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

                // Lấy giá review mới nhất
                var pricePerReview = await db.Set<ReviewFee>()
                    .OrderByDescending(x => x.ReviewFeeId)
                    .Select(x => (double?)x.Price)
                    .FirstOrDefaultAsync() ?? 0d;

                // Lấy tất cả reviews mà không cần lọc theo ngày
                var reviewsQuery = db.Set<Review>()
                    .Include(r => r.Record)
                    .Include(r => r.ReviewerProfile)
                        .ThenInclude(rp => rp.User)
                    .Where(r => r.Record != null &&
                                (r.Status == "Completed" || r.Status == "Approved" || r.Status == "Done"));

                // Tính tổng số review và tổng thu nhập
                var totalReviews = await reviewsQuery.CountAsync();
                var totalReviewers = await reviewsQuery
                    .Select(r => r.ReviewerProfileId)
                    .Distinct()
                    .CountAsync();

                var totalIncome = totalReviews * pricePerReview;

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy thống kê thu nhập reviewer thành công.";
                dto.Data = new
                {
                    TotalReviews = totalReviews,
                    TotalIncome = totalIncome,
                    TotalReviewer = totalReviewers,
                    PricePerReview = pricePerReview
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
