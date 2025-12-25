using AESP.API.Helpers;
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
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<ServicePackage> _packageRepository;
        private readonly IGenericRepository<Transaction> _transactionRepository;
        private readonly IGenericRepository<Purchase> _purchaseRepository;
        private readonly IAdminReviewerService _adminReviewerService;

        public AdminDashboardService(
            IGenericRepository<User> userRepository,
            IGenericRepository<ServicePackage> packageRepository,
            IGenericRepository<Transaction> transactionRepository,
            IGenericRepository<Purchase> purchaseRepository,
            IAdminReviewerService adminReviewerService
        )
        {
            _userRepository = userRepository;
            _packageRepository = packageRepository;
            _transactionRepository = transactionRepository;
            _purchaseRepository = purchaseRepository;
            _adminReviewerService = adminReviewerService;
        }

        public async Task<ResponseDTO> GetPackagesByMonthAsync(int year)
        {
            var dto = new ResponseDTO();

            try
            {
                if (year <= 0) year = DateTimeHelper.NowVN().Year;

                var db = _packageRepository.GetDbContext();

                // ✅ LẤY DỮ LIỆU NGƯỜI MUA SERVICE PACKAGE THEO THÁNG
                var monthlyData = await db.Transactions
                    .Where(t =>
                        t.Status == "Paid" &&
                        t.Type == "Deposit" &&
                        t.ServicePackageId != null &&
                        t.CreatedTransaction.Year == year
                    )
                    .GroupBy(t => t.CreatedTransaction.Month)
                    .Select(g => new
                    {
                        Month = g.Key,

                        // ✅ ĐẾM SỐ NGƯỜI MUA (DISTINCT USER)
                        TotalPurchases = g.Count()
                    })
                    .ToListAsync();

                // ✅ ĐỔ FILL ĐỦ 12 THÁNG CHO BIỂU ĐỒ
                var result = Enumerable.Range(1, 12)
                    .Select(m => new MonthlyStatDTO
                    {
                        Month = m,
                        Count = monthlyData.FirstOrDefault(x => x.Month == m)?.TotalPurchases ?? 0,  // ← Dùng TotalPurchases
                        Revenue = 0 // FE hiện chỉ cần biểu đồ lượt mua → để 0
                    })
                    .ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = $"Thống kê số lượt mua service package theo tháng năm {year} thành công.";
                dto.Data = result;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi thống kê service package theo tháng: " + ex.Message;
            }

            return dto;
        }


        public async Task<ResponseDTO> GetPendingReviewersAsync(int pageNumber, int pageSize)
        {
            return await _adminReviewerService.GetPendingReviewersAsync(pageNumber, pageSize);
        }

        public async Task<ResponseDTO> GetRevenueByMonthAsync(int year)
        {
            var dto = new ResponseDTO();
            try
            {
                if (year <= 0)
                    year = DateTimeHelper.NowVN().Year;

                // ✅ PHẢI DÙNG TRANSACTION
                var db = _transactionRepository.GetDbContext();

                var monthlyData = await db.Transactions
                    .Where(t =>
                        t.CreatedTransaction.Year == year &&
                        t.Status == "Paid" &&
                        t.Type == "Deposit")           // ✅ tiền nạp thật
                    .GroupBy(t => t.CreatedTransaction.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Revenue = g.Sum(x => x.AmountMoney) // ✅ tiền VNĐ
                    })
                    .ToListAsync();

                var result = Enumerable.Range(1, 12)
                    .Select(m => new MonthlyStatDTO
                    {
                        Month = m,
                        Revenue = monthlyData.FirstOrDefault(x => x.Month == m)?.Revenue ?? 0,
                        Count = 0
                    })
                    .ToList();

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = $"Thống kê doanh thu năm {year} thành công.";
                dto.Data = result;
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy thống kê doanh thu: " + ex.Message;
            }

            return dto;
        }



        public async Task<ResponseDTO> GetSummaryAsync()
        {
            var dto = new ResponseDTO();
            try
            {
                var db = _userRepository.GetDbContext();

                int totalLearners = await db.Users.CountAsync(u => u.Role == "LEARNER");
                int totalActiveLearners = await db.Users.CountAsync(u => u.Role == "LEARNER" && u.Status == "Active");
                int totalPackages = await db.ServicePackages.CountAsync();
                decimal totalRevenue = await db.Transactions
    .Where(t => t.Status == "Paid" && t.Type == "Deposit")
                    .SumAsync(t => (decimal?)t.AmountMoney) ?? 0;

                dto.IsSucess = true;
                dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
                dto.Message = "Lấy dữ liệu tổng quan thành công.";
                dto.Data = new DashboardSummaryDTO
                {
                    TotalLearners = totalLearners,
                    TotalActiveLearners = totalActiveLearners,
                    TotalServicePackages = totalPackages,
                    TotalRevenue = totalRevenue
                };
            }
            catch (Exception ex)
            {
                dto.IsSucess = false;
                dto.BusinessCode = BusinessCode.EXCEPTION;
                dto.Message = "Lỗi khi lấy tổng quan Dashboard: " + ex.Message;
            }
            return dto;
        }



    }
}
