using AESP.Common.DTOs;
using AESP.Common.Enums;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Repository.Repositories;
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
        private readonly IRefreshTokenRepository _refreshTokenRepository;

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
            IConfiguration configuration,
            IRefreshTokenRepository refreshTokenRepository)
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
            _refreshTokenRepository = refreshTokenRepository; 
        }

        public async Task<LoginResult> SignUpAsync(SignUpDto dto)
        {
            var existingUser = await _userRepository.GetByExpression(u => u.PhoneNumber == dto.PhoneNumber);
            if (existingUser != null)
                return new LoginResult { Success = false, Message = "Số điện thoại này đã tồn tại." };

            var role = await _roleRepository.GetById((int)dto.Role);
            if (role == null)
                return new LoginResult { Success = false, Message = "Role không hợp lệ." };

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                RoleId = (int)dto.Role,
                Status = "InActive"
            };

            await _userRepository.Insert(user);
            await _unitOfWork.SaveChangeAsync();

            if (dto.Role == UserRole.LEARNER)
            {
                var learnerProfile = new LearnerProfile
                {
                    LearnerProfileId = Guid.NewGuid(),
                    UserId = user.UserId
                };

                await _learnerProfileRepository.Insert(learnerProfile);
                await _unitOfWork.SaveChangeAsync();
            }

            if (dto.Role == UserRole.MENTOR)
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


        public async Task<LoginResult> SignInAsync(LoginRequest request, string? ipAddress, string? deviceInfo)
        {
            var user = await _userRepository.GetByExpression(u => u.PhoneNumber == request.PhoneNumber, u => u.Role);
            if (user == null)
                return new LoginResult { Success = false, Message = "Số điện thoại này chưa được đăng ký." };
            if (user.Status == "InActive")
                return new LoginResult { Success = false, Message = "Tài khoản này chưa xác minh email. Vui lòng kích hoạt để được sử dụng." };
            if (user.Status == "Banned")
                return new LoginResult { Success = false, Message = "Tài khoản đã bị khóa." };

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return new LoginResult { Success = false, Message = "Mật khẩu không đúng." };

            var accessToken = _jwtService.GenerateAccessToken(user);

            var result = await _refreshTokenRepository.GetAllDataByExpression(
                r => r.UserId == user.UserId && !r.Revoked,
                    0, 0, null, true
                );

            foreach (var old in result.Items)
            {
                old.Revoked = true;
                await _refreshTokenRepository.Update(old);
            }


            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                Token = refreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(2), 
                Revoked = false,
                IpAddress = ipAddress ?? "unknown",
                DeviceInfo = deviceInfo ?? "unknown"

            };

            await _refreshTokenRepository.Insert(refreshTokenEntity);
            await _unitOfWork.SaveChangeAsync();

        
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
            else if (user.RoleId == 3) 
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
                Token = accessToken,
                RefreshToken = refreshToken,
                RoleName = user.Role?.RoleName,
                IsPlacementTestDone = isPlacementTestDone,
                IsGoalSet = isGoalSet,
                IsProfileCompleted = isProfileCompleted
            };
        }


        public async Task<LoginResult> RenewTokenAsync(string accessToken, string refreshToken, string? ipAddress, string? deviceInfo)
        {
            {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken? jwtToken;

                try
                {
                    jwtToken = handler.ReadJwtToken(accessToken);
                }
                catch
                {
                    return new LoginResult { Success = false, Message = "AccessToken không hợp lệ." };
                }

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (!Guid.TryParse(userIdClaim, out var userId))
                    return new LoginResult { Success = false, Message = "Không tìm thấy UserId trong token." };

                var user = await _userRepository.GetById(userId);
                if (user == null)
                    return new LoginResult { Success = false, Message = "Người dùng không tồn tại." };


                var storedToken = await _refreshTokenRepository.GetByExpression(r =>
                    r.UserId == userId && r.Token == refreshToken);

                if (storedToken == null || storedToken.Revoked || storedToken.ExpiredAt <= DateTime.UtcNow)
                {
                    return new LoginResult { Success = false, Message = "Refresh token không hợp lệ hoặc đã hết hạn." };
                }


                storedToken.Revoked = true;
                await _refreshTokenRepository.Update(storedToken);

                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    Token = newRefreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddMinutes(2),
                    Revoked = false,
                    IpAddress = ipAddress ?? "unknown",
                    DeviceInfo = deviceInfo ?? "unknown"
                };


                await _refreshTokenRepository.Insert(refreshTokenEntity);
                await _unitOfWork.SaveChangeAsync();

                return new LoginResult
                {
                    Success = true,
                    Message = "Renew thành công",
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    RoleName = user.Role?.RoleName
                };
            }
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

        public async Task<(bool Success, string Message)> LogoutAsync(Guid userId)
        {
            var tokens = await _refreshTokenRepository.GetAllDataByExpression(
                r => r.UserId == userId && !r.Revoked,
                0, 0, null, true
            );

            foreach (var token in tokens.Items)
            {
                token.Revoked = true;
                await _refreshTokenRepository.Update(token);
            }

            await _unitOfWork.SaveChangeAsync();
            return (true, "Đăng xuất thành công");
        }




        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private bool VerifyPassword(string password, string storedHash) =>
            HashPassword(password) == storedHash;


        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

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
