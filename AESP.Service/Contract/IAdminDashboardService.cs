using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAdminDashboardService
    {
        //Task<ResponseDTO> GetSummaryAsync();
        //Task<ResponseDTO> GetPackagesByMonthAsync(int year);
        //Task<ResponseDTO> GetRevenueByMonthAsync(int year);
        Task<ResponseDTO> GetPendingReviewersAsync(int pageNumber, int pageSize);
    }
}
