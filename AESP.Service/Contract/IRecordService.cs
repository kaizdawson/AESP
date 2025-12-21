using AESP.Common.DTOs;

namespace AESP.Service.Contract
{
    public interface IRecordService
    {
        Task<ResponseDTO> SubmitRecordAsync(Guid learnerProfileId, Guid folderId, SubmitRecordDTO dto);
        Task<ResponseDTO> UpdateRecordAIResultAsync(Guid learnerProfileId, Guid recordId, UpdateRecordAIResultDTO dto);
        Task<ResponseDTO> GetRecordsByCategoryAsync(Guid learnerProfileId, Guid folderId);
        Task<ResponseDTO> DeleteRecordContentAsync(
    Guid learnerProfileId,
    Guid recordContentId);


        Task<ResponseDTO> CreateRecordContentOnlyAsync(Guid learnerProfileId, Guid folderId, CreateRecordContentOnlyDTO dto);

        Task<ResponseDTO> UpdateRecordContentAsync(Guid learnerProfileId,Guid recordContentId,UpdateRecordContentDTO dto);


        Task<ResponseDTO> SubmitRecordUpdateAsync(Guid learnerProfileId, Guid recordId, SubmitRecordUpdateDTO dto);

    }
}
