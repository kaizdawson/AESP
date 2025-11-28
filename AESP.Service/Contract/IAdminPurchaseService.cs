using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IAdminPurchaseService
    {
        Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize, string? keyword, string? type);
        Task<ResponseDTO> GetDetailAsync(Guid purchaseId);
        Task<byte[]> ExportPdfAsync();
        Task<ResponseDTO> GetDashboardAsync();
    }
}
