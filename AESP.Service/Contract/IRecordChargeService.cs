using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IRecordChargeService
    {
        Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize);
        Task<ResponseDTO> GetAllActiveAsync();
        Task<ResponseDTO> CreateAsync(RecordChargeCreateOrUpdateDto dto);
        Task<ResponseDTO> UpdateAsync(Guid id, RecordChargeCreateOrUpdateDto dto);
        Task<ResponseDTO> ToggleStatusAsync(Guid id);
        Task<ResponseDTO> DeleteAsync(Guid id);
        Task<ResponseDTO> GetDetailAsync(Guid recordChargeId, int pageNumber, int pageSize);

    }
}
