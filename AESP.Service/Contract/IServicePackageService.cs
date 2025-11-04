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
        Task<ResponseDTO> GetAllAsync(string? search);
        Task<ResponseDTO> GetAllActiveAsync();
        Task<ResponseDTO> UpdateBonusPercentAsync(Guid id, UpdateBonusPercentDto request);
    }
}
