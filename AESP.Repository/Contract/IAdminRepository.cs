using AESP.Repository.Models;

namespace AESP.Repository.Contract
{
    public interface IAdminRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(Guid id);
        Task UpdateUserAsync(User user);
        Task<List<User>> GetUsersByRoleAsync(string roleName);

    }
}
