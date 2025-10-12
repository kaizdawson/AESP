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
        Task<ResponseDTO> GetByIdAsync(Guid id);
        Task<ResponseDTO> GetListAsync();
    }
}
