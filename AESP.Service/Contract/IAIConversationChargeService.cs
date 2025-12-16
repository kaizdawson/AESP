using AESP.Common.DTOs;

namespace AESP.Service.Contract
{
    public interface IAIConversationChargeService
    {
        Task<ResponseDTO> GetAllAsync(int pageNumber, int pageSize, string? status = null);
        Task<ResponseDTO> GetAllActiveAsync();
        Task<ResponseDTO> CreateAsync(AIConversationChargeCreateOrUpdateDto dto);
        Task<ResponseDTO> UpdateAsync(Guid id, AIConversationChargeCreateOrUpdateDto dto);
        Task<ResponseDTO> ToggleStatusAsync(Guid id);

        Task<ResponseDTO> DeleteAsync(Guid id);
    }
}
