using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;


namespace AESP.Service.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public AuthService(IAuthRepository authRepository, JwtService jwtService, IEmailService emailService,IMemoryCache cache)
        {
            _authRepository = authRepository;
            _jwtService = jwtService;
            _emailService = emailService; 
            _cache = cache;
        }

        public async Task<LoginResult> SignUpAsync(SignUpDto dto, int roleId)
        {
            var existingUser = await _authRepository.GetUserByPhoneAsync(dto.PhoneNumber);
            if (existingUser != null)
                return new LoginResult { Success = false, Message = "Số điện thoại này đã tồn tại." };

            var role = await _authRepository.GetRoleByIdAsync(roleId);
            if (role == null)
                return new LoginResult { Success = false, Message = "Role không hợp lệ." };

            var passwordHash = HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Email= dto.Email,
                PasswordHash = passwordHash,
                RoleId = roleId,
                Status = "UnActive"
            };

            await _authRepository.AddUserAsync(user);

            return new LoginResult
            {
                Success = true,
                Message = "Đăng ký thành công"
            };
        }


        public async Task<LoginResult> SignInAsync(LoginRequest request)
        {
            var user = await _authRepository.GetUserByPhoneAsync(request.PhoneNumber);

            if (user == null)
                return new LoginResult { Success = false, Message = "Số điện thoại này chưa được đăng ký." };

            if (user.Status == "Banned")
                return new LoginResult { Success = false, Message = "Tài khoản đã bị khóa rồi." };

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return new LoginResult { Success = false, Message = "Mật khẩu không đúng." };

            var token = _jwtService.GenerateAccessToken(user);

            return new LoginResult
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = token
            };
        }



        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }

        public async Task<LoginResult> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken;
            try
            {
                jwtToken = handler.ReadJwtToken(dto.Token);
            }
            catch
            {
                return new LoginResult { Success = false, Message = "Token không hợp lệ" };
            }

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guidId))
                return new LoginResult { Success = false, Message = "Token không hợp lệ" };

            var user = await _authRepository.GetUserByIdAsync(guidId);
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
                await _authRepository.MarkUserVerifiedAsync(dto.Email);
                return (true, "Xác thực thành công! Tài khoản đã được Active.");
            }
            return (false, "OTP không hợp lệ hoặc đã hết hạn.");
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
