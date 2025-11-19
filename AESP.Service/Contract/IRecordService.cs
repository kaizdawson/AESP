using AESP.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Service.Contract
{
    public interface IRecordService
    {
        Task<ResponseDTO> CreateRecordAsync(Guid learnerProfileId, CreateRecordDTO dto);
        Task<ResponseDTO> DeleteRecordAsync(Guid learnerProfileId, Guid recordId);
        Task<ResponseDTO> SubmitRecordAsync(Guid learnerProfileId, Guid recordId, SubmitRecordDTO dto);
        Task<ResponseDTO> GetAllRecordsAsync(Guid learnerProfileId);
    }
}
