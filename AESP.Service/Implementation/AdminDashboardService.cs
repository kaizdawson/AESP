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

        //public async Task<ResponseDTO> GetPackagesByMonthAsync(int year)
        //{
        //    var dto = new ResponseDTO();
        //    try
        //    {
        //        if (year <= 0) year = DateTime.UtcNow.Year;

        //        var db = _purchaseRepository.GetDbContext();

        //        var monthlyData = await db.Purchases
        //            .Where(p => p.PurchaseDate.Year == year)
        //            .GroupBy(p => p.PurchaseDate.Month)
        //            .Select(g => new
        //            {
        //                Month = g.Key,
        //                Count = g.Count()
        //            })
        //            .ToListAsync();

        //        // ✅ Trả về đủ 12 tháng (nếu thiếu tháng -> gán 0)
        //        var result = Enumerable.Range(1, 12)
        //            .Select(m => new MonthlyStatDTO
        //            {
        //                Month = m,
        //                Count = monthlyData.FirstOrDefault(x => x.Month == m)?.Count ?? 0,
        //                Revenue = 0 // chỉ thống kê số gói, doanh thu phần khác
        //            })
        //            .ToList();

        //        dto.IsSucess = true;
        //        dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
        //        dto.Message = $"Thống kê gói dịch vụ bán ra trong năm {year} thành công.";
        //        dto.Data = result;
        //    }
        //    catch (Exception ex)
        //    {
        //        dto.IsSucess = false;
        //        dto.BusinessCode = BusinessCode.EXCEPTION;
        //        dto.Message = "Lỗi khi lấy thống kê gói bán theo tháng: " + ex.Message;
        //    }
        //    return dto;
        //}

        public async Task<ResponseDTO> GetPendingReviewersAsync(int pageNumber, int pageSize)
        {
            return await _adminReviewerService.GetPendingReviewersAsync(pageNumber, pageSize);
        }

        //public async Task<ResponseDTO> GetRevenueByMonthAsync(int year)
        //{
        //    var dto = new ResponseDTO();
        //    try
        //    {
        //        if (year <= 0) year = DateTime.UtcNow.Year;

        //        var db = _transactionRepository.GetDbContext();

        //        var monthlyData = await db.Transactions
        //            .Where(t => t.CreatedTransaction.Year == year && t.TransactionEnum == "Success")
        //            .GroupBy(t => t.CreatedTransaction.Month)
        //            .Select(g => new
        //            {
        //                Month = g.Key,
        //                TotalRevenue = g.Sum(x => x.Amount)
        //            })
        //            .ToListAsync();

        //        // ✅ Trả về đủ 12 tháng để FE vẽ biểu đồ liền mạch
        //        var result = Enumerable.Range(1, 12)
        //            .Select(m => new MonthlyStatDTO
        //            {
        //                Month = m,
        //                Revenue = (decimal)(monthlyData.FirstOrDefault(x => x.Month == m)?.TotalRevenue ?? 0),
        //                Count = 0
        //            })
        //            .ToList();

        //        dto.IsSucess = true;
        //        dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
        //        dto.Message = $"Thống kê doanh thu năm {year} thành công.";
        //        dto.Data = result;
        //    }
        //    catch (Exception ex)
        //    {
        //        dto.IsSucess = false;
        //        dto.BusinessCode = BusinessCode.EXCEPTION;
        //        dto.Message = "Lỗi khi lấy thống kê doanh thu: " + ex.Message;
        //    }
        //    return dto;
        //}

        //public async Task<ResponseDTO> GetSummaryAsync()
        //{
        //    var dto = new ResponseDTO();
        //    try
        //    {
        //        var db = _userRepository.GetDbContext();

        //        int totalLearners = await db.Users.CountAsync(u => u.Role == "LEARNER");
        //        int totalActiveLearners = await db.Users.CountAsync(u => u.Role == "LEARNER" && u.Status == "Active");
        //        int totalPackages = await db.ServicePackages.CountAsync();
        //        decimal totalRevenue = await db.Transactions
        //            .Where(t => t.TransactionEnum == "Success")
        //            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        //        dto.IsSucess = true;
        //        dto.BusinessCode = BusinessCode.GET_DATA_SUCCESSFULLY;
        //        dto.Message = "Lấy dữ liệu tổng quan thành công.";
        //        dto.Data = new DashboardSummaryDTO
        //        {
        //            TotalLearners = totalLearners,
        //            TotalActiveLearners = totalActiveLearners,
        //            TotalServicePackages = totalPackages,
        //            TotalRevenue = totalRevenue
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        dto.IsSucess = false;
        //        dto.BusinessCode = BusinessCode.EXCEPTION;
        //        dto.Message = "Lỗi khi lấy tổng quan Dashboard: " + ex.Message;
        //    }
        //    return dto;
        //}
    }
}
