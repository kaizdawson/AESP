using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;

namespace AESP.Service.Implementation
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
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

        public async Task<(bool Success, string Message)> CreateManagerAsync(CreateManagerDto dto)
        {
            // Kiểm tra trùng email hoặc phone
            var existingUser = await _userRepository.GetByExpression(u =>
                u.Email == dto.Email || u.PhoneNumber == dto.PhoneNumber);

            if (existingUser != null)
                return (false, "Email hoặc số điện thoại này đã tồn tại trong hệ thống.");

            // Hash mật khẩu (đơn giản hóa, có thể thay bằng bcrypt)
            using var sha = System.Security.Cryptography.SHA256.Create();
            string hashedPassword = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dto.Password)));

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                PasswordHash = hashedPassword,
                Role = "MANAGER",
                Status = "Active"
            };

            await _userRepository.Insert(user);
            await _unitOfWork.SaveChangeAsync();

            return (true, "Tạo tài khoản Manager thành công.");
        }
    }
}
