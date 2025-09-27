using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace AESP.Service.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepository;
        private readonly IGenericRepository<MentorProfile> _mentorProfileRepository;
        private readonly IGenericRepository<Assessment> _assessmentRepository;
        private readonly IGenericRepository<TeachingCertificate> _certificateRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;

        public AuthService(
            IGenericRepository<User> userRepository,
            IGenericRepository<Role> roleRepository,
            IGenericRepository<LearnerProfile> learnerProfileRepository,
            IGenericRepository<MentorProfile> mentorProfileRepository,
            IGenericRepository<Assessment> assessmentRepository,
            IGenericRepository<TeachingCertificate> certificateRepository,
            IUnitOfWork unitOfWork,
            JwtService jwtService,
            IEmailService emailService,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _learnerProfileRepository = learnerProfileRepository;
            _mentorProfileRepository = mentorProfileRepository;
            _assessmentRepository = assessmentRepository;
            _certificateRepository = certificateRepository;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _emailService = emailService;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task<LoginResult> SignUpAsync(SignUpDto dto, int roleId)
        {
            var existingUser = await _userRepository.GetByExpression(u => u.PhoneNumber == dto.PhoneNumber);
            if (existingUser != null)
                return new LoginResult { Success = false, Message = "Số điện thoại này đã tồn tại." };

            var role = await _roleRepository.GetById(roleId);
            if (role == null)
                return new LoginResult { Success = false, Message = "Role không hợp lệ." };

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                RoleId = roleId,
                Status = "InActive"
            };

            await _userRepository.Insert(user);
            await _unitOfWork.SaveChangeAsync();

            if (roleId == 2)
            {
                var learnerProfile = new LearnerProfile
                {
                    LearnerProfileId = Guid.NewGuid(),
                    UserId = user.UserId
                };

                await _learnerProfileRepository.Insert(learnerProfile);
                await _unitOfWork.SaveChangeAsync();
            }

            if (roleId == 3)
            {
                var mentorProfile = new MentorProfile
                {
                    MentorProfileId = Guid.NewGuid(),
                    UserId = user.UserId
                };

                await _mentorProfileRepository.Insert(mentorProfile);
                await _unitOfWork.SaveChangeAsync();
            }

            return new LoginResult { Success = true, Message = "Đăng ký thành công" };
        }

        public async Task<LoginResult> SignInAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByExpression(u => u.PhoneNumber == request.PhoneNumber, u => u.Role);
            if (user == null)
                return new LoginResult { Success = false, Message = "Số điện thoại này chưa được đăng ký." };

            if (user.Status == "Banned")
                return new LoginResult { Success = false, Message = "Tài khoản đã bị khóa." };

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return new LoginResult { Success = false, Message = "Mật khẩu không đúng." };

            var token = _jwtService.GenerateAccessToken(user);

            bool? isPlacementTestDone = null;
            bool? isGoalSet = null;
            bool? isProfileCompleted = null;

            if (user.RoleId == 2) 
            {
                var learnerProfile = await _learnerProfileRepository
                    .GetByExpression(lp => lp.UserId == user.UserId);

                if (learnerProfile != null)
                {
                    isGoalSet = !string.IsNullOrEmpty(learnerProfile.Goal);

                    var assessment = await _assessmentRepository
                        .GetByExpression(a => a.LearnerProfileId == learnerProfile.LearnerProfileId);

                    isPlacementTestDone = assessment != null;
                }
            }
            else if (user.RoleId == 3) // Mentor
            {
                var mentorProfile = await _mentorProfileRepository
                    .GetByExpression(mp => mp.UserId == user.UserId);

                if (mentorProfile != null)
                {
                    var certificate = await _certificateRepository
                        .GetByExpression(c => c.MentorProfileId == mentorProfile.MentorProfileId);

                    isProfileCompleted = certificate != null;
                }
            }


            return new LoginResult
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token,
                RoleName = user.Role?.RoleName,
                IsPlacementTestDone = isPlacementTestDone,
                IsGoalSet = isGoalSet,
                IsProfileCompleted = isProfileCompleted
            };
        }

        public async Task<LoginResult> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken;

            try { jwtToken = handler.ReadJwtToken(dto.Token); }
            catch { return new LoginResult { Success = false, Message = "Token không hợp lệ" }; }

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (!Guid.TryParse(userId, out var guidId))
                return new LoginResult { Success = false, Message = "Token không hợp lệ" };

            var user = await _userRepository.GetById(guidId);
            if (user == null)
                return new LoginResult { Success = false, Message = "Người dùng không tồn tại" };

            var newToken = _jwtService.GenerateAccessToken(user);
            return new LoginResult { Success = true, Message = "Refresh thành công", Token = newToken };
        }

        public async Task SendOtpAsync(string email)
        {
            var otp = OtpGenerator.GenerateOtp();
            _cache.Set(email, otp, TimeSpan.FromMinutes(5));
            await _emailService.SendEmailAsync(email, "Xác thực tài khoản", $"Mã OTP của bạn là: {otp}");
        }

        public async Task<(bool Success, string Message)> VerifyOtpAsync(OtpVerifyDto dto)
        {
            if (_cache.TryGetValue(dto.Email, out string cachedOtp) && cachedOtp == dto.Otp)
            {
                var user = await _userRepository.GetByExpression(u => u.Email == dto.Email);
                if (user != null)
                {
                    user.Status = "Active";
                    await _userRepository.Update(user);
                    await _unitOfWork.SaveChangeAsync();
                }
                return (true, "Xác thực thành công! Tài khoản đã được Active.");
            }
            return (false, "OTP không hợp lệ hoặc đã hết hạn.");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) return (false, "Người dùng không tồn tại.");

            if (!VerifyPassword(dto.CurrentPassword, user.PasswordHash))
                return (false, "Mật khẩu hiện tại không đúng.");

            if (dto.NewPassword != dto.ConfirmPassword)
                return (false, "Mật khẩu xác nhận không khớp.");

            user.PasswordHash = HashPassword(dto.NewPassword);
            await _userRepository.Update(user);
            await _unitOfWork.SaveChangeAsync();

            return (true, "Đổi mật khẩu thành công.");
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _userRepository.GetByExpression(u => u.Email == dto.Email);
            if (user == null) return (false, "Email không tồn tại trong hệ thống.");

            var token = Guid.NewGuid().ToString("N");
            _cache.Set($"reset_{dto.Email}", token, TimeSpan.FromMinutes(15));

            var resetLink = $"https://your-frontend.com/reset-password?email={dto.Email}&token={token}";
            await _emailService.SendEmailAsync(dto.Email, "Reset Password",
                $"<p>Click để reset mật khẩu:</p><a href='{resetLink}'>Đặt lại mật khẩu</a>");

            return (true, "Link đặt lại mật khẩu đã được gửi tới email.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordByLinkAsync(ResetPasswordByLinkDto dto)
        {
            if (!_cache.TryGetValue($"reset_{dto.Email}", out string? cachedToken) || cachedToken != dto.Token)
                return (false, "Token không hợp lệ hoặc đã hết hạn.");

            if (dto.NewPassword != dto.ConfirmPassword)
                return (false, "Mật khẩu xác nhận không khớp.");

            var user = await _userRepository.GetByExpression(u => u.Email == dto.Email);
            if (user == null) return (false, "Người dùng không tồn tại.");

            user.PasswordHash = HashPassword(dto.NewPassword);
            await _userRepository.Update(user);
            await _unitOfWork.SaveChangeAsync();

            _cache.Remove($"reset_{dto.Email}");

            return (true, "Đặt lại mật khẩu thành công.");
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPassword(string password, string storedHash) =>
            HashPassword(password) == storedHash;
    }

    public static class OtpGenerator
    {
        public static string GenerateOtp(int length = 6)
        {
            var random = new Random();
            return string.Concat(Enumerable.Range(0, length).Select(_ => random.Next(0, 10).ToString()));
        }
    }
}
