using AESP.Common.DTOs;

namespace AESP.Service.Contract
{
    public interface IRecordService
    {
        Task<ResponseDTO> SubmitRecordAsync(Guid learnerProfileId, Guid folderId, SubmitRecordDTO dto);
        Task<ResponseDTO> DeleteRecordAsync(Guid learnerProfileId, Guid recordId);
        Task<ResponseDTO> GetRecordsByCategoryAsync(Guid learnerProfileId, Guid folderId);
    }

}
