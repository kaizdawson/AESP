using AESP.Common.DTOs;
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

            var existingUserByEmail = await _userRepository.GetByExpression(u => u.Email == dto.Email);
            if (existingUserByEmail != null)
            {
        
                if (!string.IsNullOrEmpty(existingUserByEmail.FirebaseUid))
                    return new LoginResult { Success = false, Message = "Email này đã được đăng ký bằng Google. Vui lòng đăng nhập bằng Google." };

         
                return new LoginResult { Success = false, Message = "Email này đã tồn tại." };
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Role = dto.Role.ToString(),
                Status = "InActive"
            };

            await _userRepository.Insert(user); 
            await _unitOfWork.SaveChangeAsync();

            if (dto.Role.ToUpper() == "LEARNER")
            {
                var learnerProfile = new LearnerProfile
                {
                    LearnerProfileId = Guid.NewGuid(),
                    UserId = user.UserId
                };

                await _learnerProfileRepository.Insert(learnerProfile);
                await _unitOfWork.SaveChangeAsync();
            }

            if (dto.Role.ToUpper() == "MENTOR")
            {
                var mentorProfile = new MentorProfile
                {
                    MentorProfileId = Guid.NewGuid(),
                    UserId = user.UserId
                };

                await _mentorProfileRepository.Insert(mentorProfile);
                await _unitOfWork.SaveChangeAsync();
            }

            var otp = OtpGenerator.GenerateOtp();
            _cache.Set(user.Email, otp, TimeSpan.FromMinutes(2));
            await _emailService.SendEmailAsync(user.Email, "Xác thực tài khoản", $"Mã OTP của bạn là: {otp}");

            return new LoginResult { Success = true, Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực OTP.", Email = user.Email };
        }


        public async Task<LoginResult> SignInAsync(LoginRequest request, string? ipAddress, string? deviceInfo)
        {
            var user = await _userRepository.GetByExpression(u => u.Email == request.Email);
            if (user == null)
                return new LoginResult { Success = false, Message = "Email này chưa được đăng ký." };

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

            if (user.Role.ToUpper() == "LEARNER")
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
            else if (user.Role.ToUpper() == "MENTOR")
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
                Role = user.Role,
                IsPlacementTestDone = isPlacementTestDone,
                IsGoalSet = isGoalSet,
                IsProfileCompleted = isProfileCompleted
            };
        }


        public async Task<LoginResult> RenewTokenAsync(string refreshToken, string? ipAddress, string? deviceInfo)
        {
            var storedToken = await _refreshTokenRepository.GetByExpression(r => r.Token == refreshToken);

            if (storedToken == null || storedToken.Revoked || storedToken.ExpiredAt <= DateTime.UtcNow)
            {
                return new LoginResult { Success = false, Message = "Refresh token không hợp lệ hoặc đã hết hạn." };
            }

            var user = await _userRepository.GetById(storedToken.UserId);
            if (user == null)
                return new LoginResult { Success = false, Message = "Người dùng không tồn tại." };

           
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
                Role = user.Role
            };
        }




        public async Task SendOtpAsync(string email)
        {
            var otp = OtpGenerator.GenerateOtp();
            _cache.Set(email, otp, TimeSpan.FromMinutes(2));
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

        public async Task<(bool Success, string Message)> LogoutAsync(string refreshToken)
        {
            var storedToken = await _refreshTokenRepository.GetByExpression(r => r.Token == refreshToken);

            if (storedToken == null || storedToken.Revoked)
                return (false, "Refresh token không hợp lệ hoặc đã logout rồi.");

            storedToken.Revoked = true;
            await _refreshTokenRepository.Update(storedToken);
            await _unitOfWork.SaveChangeAsync();

            return (true, "Đăng xuất thành công.");
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

        public async Task<LoginResult> GoogleSignInAsync(string idToken, string? ipAddress, string? deviceInfo)
        {
            try
            {
                var decodedToken = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

                string firebaseUid = decodedToken.Uid;
                string email = decodedToken.Claims.ContainsKey("email") ? decodedToken.Claims["email"].ToString()! : "";
                string name = decodedToken.Claims.ContainsKey("name") ? decodedToken.Claims["name"].ToString()! : "";
                string avatar = decodedToken.Claims.ContainsKey("picture") ? decodedToken.Claims["picture"].ToString()! : "";

                if (string.IsNullOrEmpty(firebaseUid))
                    return new LoginResult { Success = false, Message = "Không lấy được Firebase UID." };

                User? user = null;

               
                var usersByUid = await _userRepository.GetAllDataByExpression(u => u.FirebaseUid == firebaseUid, 0, 0, null, true);
                user = usersByUid.Items.FirstOrDefault();

             
                if (user == null && !string.IsNullOrEmpty(email))
                {
                    var usersByEmail = await _userRepository.GetAllDataByExpression(u => u.Email == email, 0, 0, null, true);
                    user = usersByEmail.Items.FirstOrDefault();

                    if (user != null)
                    {
                        // thêm FirebaseUID nè
                        user.FirebaseUid = firebaseUid;
                        if (user.Status == "InActive") user.Status = "Active";
                        await _userRepository.Update(user);
                        await _unitOfWork.SaveChangeAsync();
                    }
                }

                
                if (user == null)
                {
                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        FirebaseUid = firebaseUid,
                        FullName = string.IsNullOrEmpty(name) ? "New Learner" : name,
                        Email = email,
                        PhoneNumber = "",
                        AvatarUrl = avatar,
                        PasswordHash = HashPassword(Guid.NewGuid().ToString()), // random password
                        Role = "LEARNER",
                        Status = "Active"
                    };

                    await _userRepository.Insert(user);
                    await _unitOfWork.SaveChangeAsync();

                    var learnerProfile = new LearnerProfile
                    {
                        LearnerProfileId = Guid.NewGuid(),
                        UserId = user.UserId
                    };
                    await _learnerProfileRepository.Insert(learnerProfile);
                    await _unitOfWork.SaveChangeAsync();
                }
                else
                {
                    // Nếu user tồn tại thì sao nào. up cái nào thiếu chứ seo
                    bool changed = false;

                    if (string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(email))
                    {
                        user.Email = email;
                        changed = true;
                    }
                    if (string.IsNullOrEmpty(user.FullName) && !string.IsNullOrEmpty(name))
                    {
                        user.FullName = name;
                        changed = true;
                    }
                    if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(avatar))
                    {
                        user.AvatarUrl = avatar;
                        changed = true;
                    }
                    if (user.Status == "InActive")
                    {
                        user.Status = "Active";
                        changed = true;
                    }

                    if (changed)
                    {
                        await _userRepository.Update(user);
                        await _unitOfWork.SaveChangeAsync();
                    }
                }

                
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken();

                var refreshTokenEntity = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.UserId,
                    Token = refreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiredAt = DateTime.UtcNow.AddDays(7),
                    Revoked = false,
                    IpAddress = ipAddress ?? "unknown",
                    DeviceInfo = deviceInfo ?? "unknown"
                };

                await _refreshTokenRepository.Insert(refreshTokenEntity);
                await _unitOfWork.SaveChangeAsync();

                return new LoginResult
                {
                    Success = true,
                    Message = "Đăng nhập Google thành công",
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    Role = user.Role
                };
            }
            catch (Exception ex)
            {
                return new LoginResult { Success = false, Message = $"Google sign-in failed: {ex.InnerException?.Message ?? ex.Message}" };
            }
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
