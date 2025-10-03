using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;

namespace AESP.Service.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(
            IGenericRepository<User> userRepository,
            IGenericRepository<Role> roleRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var result = await _userRepository.GetAllDataByExpression(
                null, 0, 0, null, true, u => u.Role);

            return result.Items
                .Where(u => u.Role != "Admin")
                .Select(ToDto)
                .ToList();
        }

        public async Task<List<UserDto>> GetLearnersAsync()
        {
            var result = await _userRepository.GetAllDataByExpression(
                     u => u.Role == "Learner", 0, 0, null, true);


            return result.Items.Select(ToDto).ToList();
        }

        public async Task<List<UserDto>> GetMentorsAsync()
        {
            var result = await _userRepository.GetAllDataByExpression(
        u => u.Role == "Mentor", 0, 0, null, true);

            return result.Items.Select(ToDto).ToList();
        }

        public async Task<string?> ToggleUserStatusAsync(Guid userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) return "User not found";

            string roleName = string.IsNullOrEmpty(user.Role) ? "Unknown" : user.Role;
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

            await _userRepository.Update(user);
            await _unitOfWork.SaveChangeAsync();

            return message;
        }

        private static UserDto ToDto(User u) => new UserDto
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Status = u.Status,
            Role = u.Role
        };
    }
}
