using AESP.Common.DTOs;
using AESP.Repository.Models;

namespace AESP.Service.Contract
{
    public interface IAdminService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<string?> ToggleUserStatusAsync(Guid userId);

        Task<List<UserDto>> GetLearnersAsync();
        Task<List<UserDto>> GetMentorsAsync();
        Task<(bool Success, string Message)> CreateManagerAsync(CreateManagerDto dto);
    }
}
