using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Contract
{
    public interface IServicePackageService
    {
        Task<ResponseDTO> CreateAsync(CreateServicePackageDto request);
        Task<ResponseDTO> UpdateAsync(Guid id, UpdateServicePackageDto request);
        Task<ResponseDTO> DeleteAsync(Guid id);
        Task<ResponseDTO> ToggleStatusAsync(Guid id);
        Task<ResponseDTO> GetAllAsync(string? search, int pageNumber = 1, int pageSize = 10, string? filter = null);
        Task<ResponseDTO> GetAllActiveAsync();
        //Task<ResponseDTO> GetServicePackageStatisticAsync();
        Task<ResponseDTO> GetBuyersOfServicePackageAsync(Guid servicePackageId, string? search, int pageNumber = 1, int pageSize = 10);
       // Task<ResponseDTO> UpdateBonusPercentAsync(Guid id, UpdateBonusPercentDto request);
    }
}
