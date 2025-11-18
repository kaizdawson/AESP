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

        public async Task<ResponseDTO> GetReviewerDetailAsync(Guid reviewerProfileId)
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _unitOfWork.GetDbContext();

                var incomeQuery = db.Set<TransferTransaction>()
                    .Where(t =>
                        t.ReviewerProfileId == reviewerProfileId &&
                        t.Status == "Completed");

                var totalIncome = await incomeQuery.SumAsync(t => (int?)t.AmountCoin) ?? 0;
                var totalReviews = await incomeQuery.CountAsync();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy chi tiết reviewer thành công.";
                dto.Data = new
                {
                    ReviewerProfileId = reviewerProfileId,
                    TotalIncome = totalIncome,
                    TotalReviews = totalReviews
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy chi tiết: " + ex.Message;
            }

            return dto;
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
