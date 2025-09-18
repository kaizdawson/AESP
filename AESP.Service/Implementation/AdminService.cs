using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;

namespace AESP.Service.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _adminRepository.GetAllUsersAsync();
            return users
                .Where(u => u.Role?.RoleName != "Admin")
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Status = u.Status,
                    RoleName = u.Role?.RoleName ?? "Unknown"
                })
                .ToList();
        }

        public async Task<List<UserDto>> GetLearnersAsync()
        {
            var users = await _adminRepository.GetUsersByRoleAsync("Learner");

            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Status = u.Status,
                RoleName = u.Role?.RoleName ?? "Unknown"
            }).ToList();
        }

        public async Task<List<UserDto>> GetMentorsAsync()
        {
            var users = await _adminRepository.GetUsersByRoleAsync("Mentor");

            return users.Select(u => new UserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Status = u.Status,
                RoleName = u.Role?.RoleName ?? "Unknown"
            }).ToList();
        }


        public async Task<string?> ToggleUserStatusAsync(Guid userId)
        {
            var user = await _adminRepository.GetUserByIdAsync(userId);
            if (user == null) return "User not found";

            string roleName = user.Role?.RoleName ?? "Unknown";
            string message;

            if (user.Status == "Active")
            {
                user.Status = "Banned";
                message = $"Đã chặn {roleName} {user.FullName}";
            }
            else
            {
                user.Status = "Active";
                message = $"Đã mở khóa {roleName} {user.FullName}";
            }

            await _adminRepository.UpdateUserAsync(user);
            return message;
        }


    }
}
