using AESP.Repository.Models;

namespace AESP.Repository.Contract
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByPhoneAsync(string phoneNumber);
        Task AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(Guid id);

        Task MarkUserVerifiedAsync(string email);

        Task UpdateUserAsync(User user);

        Task<User?> GetUserByEmailAsync(string email);

    }
}
