using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Repository.Repositories;
using AESP.Service.Contract;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AESP.Service.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepository;
        private readonly IGenericRepository<ReviewerProfile> _reviewerProfileRepository;
        private readonly IGenericRepository<Assessment> _assessmentRepository;
        private readonly IGenericRepository<Certificate> _certificateRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IGenericRepository<ProgressAnalytics> _progressAnalyticsRepository;

        public AuthService(
            IGenericRepository<User> userRepository,
            IGenericRepository<LearnerProfile> learnerProfileRepository,
            IGenericRepository<ReviewerProfile> reviewerProfileRepository,
            IGenericRepository<Assessment> assessmentRepository,
            IGenericRepository<Certificate> certificateRepository,
            IUnitOfWork unitOfWork,
            JwtService jwtService,
            IEmailService emailService,
            IMemoryCache cache,
            IConfiguration configuration,
            IRefreshTokenRepository refreshTokenRepository,
            IGenericRepository<ProgressAnalytics> progressAnalyticsRepository)
        {
            _userRepository = userRepository;
            _learnerProfileRepository = learnerProfileRepository;
            _reviewerProfileRepository = reviewerProfileRepository;
            _assessmentRepository = assessmentRepository;
            _certificateRepository = certificateRepository;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _emailService = emailService;
            _cache = cache;
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _progressAnalyticsRepository = progressAnalyticsRepository;
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
                Status = "InActive",
                CoinBalance = 0
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

                var progress = new ProgressAnalytics
                {
                    ProgressAnalyticsId = Guid.NewGuid(),
                    DateRecorded = DateTime.UtcNow,
                    SpeakingTime = 0,
                    SessionsCompleted = 0,
                    PronunciationScoreAvg = 0,
                    LearnerProfileId = learnerProfile.LearnerProfileId
                };

                await _progressAnalyticsRepository.Insert(progress);
                await _unitOfWork.SaveChangeAsync();

            }

            var otp = OtpGenerator.GenerateOtp();
            _cache.Set(user.Email, otp, TimeSpan.FromMinutes(2));
            await _emailService.SendEmailAsync(user.Email, "Xác thực tài khoản", $"Mã OTP của bạn là: {otp}");

            return new LoginResult { Success = true, Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực OTP.", Email = user.Email };
        }


        public async Task<LoginResult> SignInAsync(LoginRequest request, string? ipAddress, string? deviceInfo)
        {
            if (string.IsNullOrWhiteSpace(request.Role))
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Role không được để trống."
                };
            }

            var user = await _userRepository.GetByExpression(u => u.Email == request.Email);
            if (user == null)
                return new LoginResult { Success = false, Message = "Email này chưa được đăng ký." };

            if (!string.Equals(user.Role, request.Role, StringComparison.OrdinalIgnoreCase))
            {
                return new LoginResult
                {
                    Success = false,
                    Message = $"Role không phù hợp. Tài khoản này thuộc role '{user.Role}'."
                };
            }

            if (user.Status == "InActive")
                return new LoginResult { Success = false, Message = "Tài khoản này chưa xác minh email. Vui lòng kích hoạt để được sử dụng." };
            if (user.Status == "Banned")
                return new LoginResult { Success = false, Message = "Tài khoản đã bị khóa." };

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return new LoginResult { Success = false, Message = "Mật khẩu không đúng." };

            // ---- REVOKE OLD TOKENS ----
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
                ExpiredAt = DateTime.UtcNow.AddDays(7),
                Revoked = false,
                IpAddress = ipAddress ?? "unknown",
                DeviceInfo = deviceInfo ?? "unknown"
            };

            await _refreshTokenRepository.Insert(refreshTokenEntity);
            await _unitOfWork.SaveChangeAsync();

            bool isPlacementTestDone = false;
            bool isReviewerActive = false;
            string? reviewerStatus = null;

            // ✅ CHUẨN BỊ DANH SÁCH CLAIMS CHUNG CHO TẤT CẢ TOKEN
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
        new Claim(ClaimTypes.Role, user.Role),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim("UserId", user.UserId.ToString())
    };

            // ---- LEARNER ----
            if (user.Role.ToUpper() == "LEARNER")
            {
                var learnerProfile = await _learnerProfileRepository
                    .GetByExpression(lp => lp.UserId == user.UserId);

                if (learnerProfile != null)
                {
                    // ⚡️ Thêm LearnerProfileId vào token
                    claims.Add(new Claim("LearnerProfileId", learnerProfile.LearnerProfileId.ToString()));

                    var assessment = await _assessmentRepository
                     .GetByExpression(a => a.LearnerProfileId == learnerProfile.LearnerProfileId && a.Score!= null );

                    isPlacementTestDone = assessment != null;

                }
            }

            // ---- REVIEWER ----
            if (user.Role.ToUpper() == "REVIEWER")
            {
                var reviewerProfile = await _reviewerProfileRepository.GetByExpression(r => r.UserId == user.UserId);

                if (reviewerProfile != null)
                {
                    reviewerStatus = reviewerProfile.Status;
                    var st = reviewerProfile.Status?.ToUpperInvariant();
                    isReviewerActive = st == "PENDING" || st == "ACTIVE";
                }
                else
                {
                    var newProfile = new ReviewerProfile
                    {
                        ReviewerProfileId = Guid.NewGuid(),
                        UserId = user.UserId,
                        Status = "Draft"
                    };

                    reviewerStatus = "Draft";

                    await _reviewerProfileRepository.Insert(newProfile);
                    await _unitOfWork.SaveChangeAsync();
                    isReviewerActive = false;
                }
            }

            // 🔍 Lấy LearnerProfileId từ claims list (nếu có)
            Guid? learnerProfileId = null;
            var learnerClaim = claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
            if (learnerClaim != null && Guid.TryParse(learnerClaim.Value, out var parsedId))
            {
                learnerProfileId = parsedId;
            }

            // ✅ Sinh token có đúng LearnerProfileId
            var accessToken = _jwtService.GenerateAccessToken(
                user,
                isPlacementTestDone,
                isReviewerActive,
                learnerProfileId,
                reviewerStatus
            );


            return new LoginResult
            {
                Success = true,
                Message = "Đăng nhập thành công",
                Token = accessToken,
                RefreshToken = refreshToken,
                Role = user.Role,
                IsPlacementTestDone = user.Role.Equals("LEARNER", StringComparison.OrdinalIgnoreCase) ? isPlacementTestDone : (bool?)null,
                IsReviewerActive = user.Role.Equals("REVIEWER", StringComparison.OrdinalIgnoreCase) ? isReviewerActive : (bool?)null,
                ReviewerStatus = user.Role.Equals("REVIEWER", StringComparison.OrdinalIgnoreCase) ? reviewerStatus : null
            };

        }


        public async Task<LoginResult> RenewTokenAsync(string refreshToken, string? ipAddress, string? deviceInfo)
        {
            var storedToken = await _refreshTokenRepository.GetByExpression(r => r.Token == refreshToken);

            if (storedToken == null || storedToken.Revoked)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Refresh token không hợp lệ."
                };
            }

            if (storedToken.ExpiredAt <= DateTime.UtcNow)
            {
                storedToken.Revoked = true;
                await _refreshTokenRepository.Update(storedToken);
                await _unitOfWork.SaveChangeAsync();

                return new LoginResult
                {
                    Success = false,
                    Message = "Refresh token đã hết hạn."
                };
            }


            var user = await _userRepository.GetById(storedToken.UserId);
            if (user == null)
                return new LoginResult { Success = false, Message = "Người dùng không tồn tại." };


            storedToken.IpAddress = ipAddress ?? storedToken.IpAddress;
            storedToken.DeviceInfo = deviceInfo ?? storedToken.DeviceInfo;
            await _refreshTokenRepository.Update(storedToken);


            bool? isPlacementTestDone = null;
            bool? isReviewerActive = null;


            if (user.Role.Equals("LEARNER", StringComparison.OrdinalIgnoreCase))
            {
                var learnerProfile = await _learnerProfileRepository.GetByExpression(lp => lp.UserId == user.UserId);
                if (learnerProfile != null)
                {
                    var assessment = await _assessmentRepository
     .GetByExpression(a => a.LearnerProfileId == learnerProfile.LearnerProfileId && a.Score != null);

                    isPlacementTestDone = assessment != null;


                }
            }

            else if (user.Role.Equals("REVIEWER", StringComparison.OrdinalIgnoreCase))
            {
                var reviewerProfile = await _reviewerProfileRepository.GetByExpression(r => r.UserId == user.UserId);
                if (reviewerProfile != null)
                {
                    var st = reviewerProfile.Status?.ToUpperInvariant();
                    isReviewerActive = st == "PENDING" || st == "ACTIVE";
                }
            }


            var newAccessToken = _jwtService.GenerateAccessToken(user, isPlacementTestDone, isReviewerActive);

            await _unitOfWork.SaveChangeAsync();


            return new LoginResult
            {
                Success = true,
                Message = "Renew thành công",
                Token = newAccessToken,
                RefreshToken = refreshToken,
                Role = user.Role,
                IsPlacementTestDone = user.Role.Equals("LEARNER", StringComparison.OrdinalIgnoreCase) ? isPlacementTestDone : (bool?)null,
                IsReviewerActive = user.Role.Equals("REVIEWER", StringComparison.OrdinalIgnoreCase) ? isReviewerActive : (bool?)null
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

                    // Nếu là reviewer → tạo profile reviewer
                    if (user.Role.ToUpper() == "REVIEWER")
                    {
                        var reviewerProfile = new ReviewerProfile
                        {
                            ReviewerProfileId = Guid.NewGuid(),
                            UserId = user.UserId,
                            Status = "Draft" // chờ admin duyệt
                        };

                        await _reviewerProfileRepository.Insert(reviewerProfile);
                        await _unitOfWork.SaveChangeAsync();
                    }
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

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return (false, "Tài khoản này đăng nhập bằng Google. Vui lòng sử dụng nút 'Đăng nhập với Google' để vào hệ thống.");
            }

            var token = _jwtService.GenerateResetPasswordToken(user.Email);
            var resetLink = $"http://localhost:3000/reset-password?token={token}";
            await _emailService.SendEmailAsync(dto.Email, "Reset Password",
                $"<p>Click để reset mật khẩu:</p><a href='{resetLink}'>Đặt lại mật khẩu</a>");


            return (true, "Link đặt lại mật khẩu đã được gửi tới email.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordByLinkAsync(string token, ResetPasswordByLinkDto dto)
        {
            // Model đã có [Compare], check này để phản hồi rõ ràng hơn (cũng OK nếu bỏ)
            if (dto.NewPassword != dto.ConfirmPassword)
                return (false, "Mật khẩu xác nhận không khớp.");

            var email = _jwtService.ValidateAndGetEmailFromResetToken(token);
            if (string.IsNullOrEmpty(email))
                return (false, "Token không hợp lệ hoặc đã hết hạn.");

            var user = await _userRepository.GetByExpression(u => u.Email == email);
            if (user == null)
                return (false, "Người dùng không tồn tại.");

            // TODO: Dùng thuật toán hash mạnh. Ví dụ với BCrypt.Net-Next:
            // user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordHash = HashPassword(dto.NewPassword);

            await _userRepository.Update(user);
            await _unitOfWork.SaveChangeAsync();

            return (true, "Đặt lại mật khẩu thành công.");
        }



        public async Task<(bool Success, string Message, string? ErrorType)> LogoutAsync(string refreshToken)
        {
            var storedToken = await _refreshTokenRepository.GetByExpression(r => r.Token == refreshToken);


            if (storedToken == null)
                return (false, "Refresh token không hợp lệ.", "Invalid");


            if (storedToken.Revoked)
                return (false, "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.", "Expired");


            if (storedToken.ExpiredAt <= DateTime.UtcNow)
                return (false, "Refresh token đã hết hạn.", "Expired");


            storedToken.Revoked = true;
            await _refreshTokenRepository.Update(storedToken);
            await _unitOfWork.SaveChangeAsync();

            return (true, "Đăng xuất thành công.", null);
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
            Guid? learnerProfileId = null;

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
                // ✅ Extra prevent login if Firebase UID belongs to Reviewer
                if (user != null && string.Equals(user.Role, "REVIEWER", StringComparison.OrdinalIgnoreCase))
                {
                    return new LoginResult
                    {
                        Success = false,
                        Message = "Tài khoản này thuộc Reviewer. Vui lòng đăng nhập qua cổng Reviewer."
                    };
                }


                if (user == null && !string.IsNullOrEmpty(email))
                {
                    var usersByEmail = await _userRepository.GetAllDataByExpression(u => u.Email == email, 0, 0, null, true);
                    user = usersByEmail.Items.FirstOrDefault();

                    // ✅ ADDED: Prevent cross-role login
                    if (user != null && string.Equals(user.Role, "REVIEWER", StringComparison.OrdinalIgnoreCase))
                    {
                        return new LoginResult
                        {
                            Success = false,
                            Message = "Email này đã được đăng ký cho tài khoản Reviewer. Vui lòng dùng tài khoản khác cho Learner."
                        };
                    }

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
                        PasswordHash = "",
                        Role = "LEARNER",
                        Status = "Active",
                        CoinBalance = 0
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

                    learnerProfileId = learnerProfile.LearnerProfileId;
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

                bool isPlacementTestDone = false;
                bool isReviewerActive = false;

                

                if (user.Role.ToUpper() == "LEARNER")
                {
                    var learnerProfile = await _learnerProfileRepository
                        .GetByExpression(lp => lp.UserId == user.UserId);

                    if (learnerProfile != null)
                    {
                        learnerProfileId = learnerProfile.LearnerProfileId;

                        var assessment = await _assessmentRepository
    .GetByExpression(a => a.LearnerProfileId == learnerProfile.LearnerProfileId && a.Score!= null);

                        isPlacementTestDone = assessment != null;


                    }
                }



                var accessToken = _jwtService.GenerateAccessToken(
    user,
    isPlacementTestDone,
    isReviewerActive,
    learnerProfileId // ✅ Truyền đúng ID vào đây
);



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
                    Role = user.Role,
                    IsPlacementTestDone = isPlacementTestDone,
                    IsReviewerActive = isReviewerActive
                };
            }
            catch (Exception ex)
            {
                return new LoginResult { Success = false, Message = $"Google sign-in failed: {ex.InnerException?.Message ?? ex.Message}" };
            }
        }

        public async Task<LoginResult> GoogleSignInReviewerAsync(string idToken, string? ipAddress, string? deviceInfo)
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

                // 1️⃣ Tìm user theo UID hoặc Email
                User? user = null;
                var usersByUid = await _userRepository.GetAllDataByExpression(u => u.FirebaseUid == firebaseUid, 0, 0, null, true);
                user = usersByUid.Items.FirstOrDefault();

                // Chặn đăng nhập nhầm role
                if (user != null && string.Equals(user.Role, "LEARNER", StringComparison.OrdinalIgnoreCase))
                {
                    return new LoginResult
                    {
                        Success = false,
                        Message = "Tài khoản này thuộc Learner. Vui lòng đăng nhập qua cổng Learner."
                    };
                }

                if (user == null && !string.IsNullOrEmpty(email))
                {
                    var usersByEmail = await _userRepository.GetAllDataByExpression(u => u.Email == email, 0, 0, null, true);
                    user = usersByEmail.Items.FirstOrDefault();

                    if (user != null && string.Equals(user.Role, "LEARNER", StringComparison.OrdinalIgnoreCase))
                    {
                        return new LoginResult
                        {
                            Success = false,
                            Message = "Email này đã được đăng ký cho tài khoản Learner. Vui lòng dùng tài khoản khác cho Reviewer."
                        };
                    }

                    if (user != null)
                    {
                        user.FirebaseUid = firebaseUid;
                        if (user.Status == "InActive") user.Status = "Active";
                        if (!string.Equals(user.Role, "REVIEWER", StringComparison.OrdinalIgnoreCase))
                            user.Role = "REVIEWER";

                        await _userRepository.Update(user);
                        await _unitOfWork.SaveChangeAsync();
                    }
                }

                // 2️⃣ Nếu chưa có thì tạo mới
                if (user == null)
                {
                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        FirebaseUid = firebaseUid,
                        FullName = string.IsNullOrEmpty(name) ? "New Reviewer" : name,
                        Email = email,
                        AvatarUrl = avatar,
                        Role = "REVIEWER",
                        Status = "Active",
                        CoinBalance = 0
                    };

                    await _userRepository.Insert(user);
                    await _unitOfWork.SaveChangeAsync();

                    // ➤ Tạo reviewerProfile (không có Wallet)
                    var reviewerProfile = new ReviewerProfile
                    {
                        ReviewerProfileId = Guid.NewGuid(),
                        UserId = user.UserId,
                        Status = "Draft"
                    };
                    await _reviewerProfileRepository.Insert(reviewerProfile);
                    await _unitOfWork.SaveChangeAsync();
                }
                else
                {
                    // 3️⃣ Cập nhật thông tin thiếu
                    bool changed = false;
                    if (string.IsNullOrEmpty(user.FullName) && !string.IsNullOrEmpty(name)) { user.FullName = name; changed = true; }
                    if (string.IsNullOrEmpty(user.AvatarUrl) && !string.IsNullOrEmpty(avatar)) { user.AvatarUrl = avatar; changed = true; }
                    if (user.Status == "InActive") { user.Status = "Active"; changed = true; }
                    if (!string.Equals(user.Role, "REVIEWER", StringComparison.OrdinalIgnoreCase))
                    {
                        user.Role = "REVIEWER";
                        changed = true;
                    }

                    if (changed)
                    {
                        await _userRepository.Update(user);
                        await _unitOfWork.SaveChangeAsync();
                    }

                    // 4️⃣ Đảm bảo reviewerProfile tồn tại
                    var existingProfile = await _reviewerProfileRepository.GetByExpression(r => r.UserId == user.UserId);
                    if (existingProfile == null)
                    {
                        var reviewerProfile = new ReviewerProfile
                        {
                            ReviewerProfileId = Guid.NewGuid(),
                            UserId = user.UserId,
                            Status = "Draft"
                        };
                        await _reviewerProfileRepository.Insert(reviewerProfile);
                        await _unitOfWork.SaveChangeAsync();
                    }
                }

                // 5️⃣ Check trạng thái reviewer
                // 5️⃣ Đảm bảo reviewerProfile luôn tồn tại + lấy trạng thái
                bool isReviewerActive = false;
                string reviewerStatus;

                var reviewer = await _reviewerProfileRepository.GetByExpression(r => r.UserId == user.UserId);

                if (reviewer == null)
                {
                    reviewer = new ReviewerProfile
                    {
                        ReviewerProfileId = Guid.NewGuid(),
                        UserId = user.UserId,
                        Status = "Draft"
                    };

                    await _reviewerProfileRepository.Insert(reviewer);
                    await _unitOfWork.SaveChangeAsync();
                }

                // 🔥 TỪ ĐÂY TRỞ XUỐNG → reviewerStatus KHÔNG BAO GIỜ NULL
                reviewerStatus = reviewer.Status;
                var st = reviewerStatus?.ToUpperInvariant();
                isReviewerActive = st == "PENDING" || st == "ACTIVE";


                // 6️⃣ Tạo Access + Refresh token
                var accessToken = _jwtService.GenerateAccessToken(user, null, isReviewerActive, null, reviewerStatus);
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
                    Message = "Đăng nhập Google (Reviewer) thành công",
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    Role = user.Role,
                    IsReviewerActive = isReviewerActive,
                    ReviewerStatus = reviewerStatus
                };
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    Success = false,
                    Message = $"Google sign-in (Reviewer) failed: {ex.InnerException?.Message ?? ex.Message}"
                };
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