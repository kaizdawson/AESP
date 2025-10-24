using AESP.Common.DTOs;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AESP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepository;
        private readonly IGenericRepository<ReviewerProfile> _reviewerProfileRepository;

        public AuthController(IAuthService authService, IConfiguration configuration, IGenericRepository<LearnerProfile> learnerProfileRepository, IGenericRepository<ReviewerProfile> reviewerProfileRepository, IGenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
            _authService = authService;
            _configuration = configuration;
            _learnerProfileRepository = learnerProfileRepository;
            _reviewerProfileRepository = reviewerProfileRepository;
        }
        private IActionResult? ValidateModel()
        {
            if (!ModelState.IsValid)
            {
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return BadRequest(new { message = firstError });
            }
            return null;
        }





        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SignUpDto dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;


            var result = await _authService.SignUpAsync(dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message, email = result.Email });
        }


   
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceInfo = Request.Headers["User-Agent"].ToString();

            var result = await _authService.SignInAsync(request, ipAddress, deviceInfo);


            if (!result.Success)
                return BadRequest(new { message = result.Message });

            var encodedToken = Uri.EscapeDataString(result.RefreshToken!);
            Response.Cookies.Append("refreshToken", encodedToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });

            return Ok(new
            {
                accessToken = result.Token,
                refreshToken = result.RefreshToken,
                message = result.Message,
                role = result.Role,
                isPlacementTestDone = result.IsPlacementTestDone,
                isGoalSet = result.IsGoalSet,
                isProfileCompleted = result.IsProfileCompleted,
                isReviewerActive = result.IsReviewerActive
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceInfo = Request.Headers["User-Agent"].ToString();

            var result = await _authService.RenewTokenAsync(dto.RefreshToken, ipAddress, deviceInfo);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(new
            {
                accessToken = result.Token,
                refreshToken = result.RefreshToken,
                message = result.Message,
                role = result.Role
            });
        }





        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] SendOtpDto dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            await _authService.SendOtpAsync(dto.Email);
            return Ok(new { message = "OTP mới đã được gửi tới email." });
        }



        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyDto dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            var result = await _authService.VerifyOtpAsync(dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            string? userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Không xác định được người dùng." });

            var result = await _authService.ChangePasswordAsync(userId, dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            var result = await _authService.ForgotPasswordAsync(dto);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [AllowAnonymous]
        [HttpPost("reset-password-by-link")]
        public async Task<IActionResult> ResetPasswordByLink([FromBody] ResetPasswordByLinkDto dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Unauthorized(new { message = "Thiếu token hoặc định dạng không hợp lệ." });

            var token = authHeader.Substring("Bearer ".Length).Trim();

            var result = await _authService.ResetPasswordByLinkAsync(token, dto);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }







        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var encoded = Request.Cookies["refreshToken"];
            var refreshToken = Uri.UnescapeDataString(encoded ?? "");

            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Thiếu refresh token trong cookie." });

            var result = await _authService.LogoutAsync(refreshToken);


            if (!result.Success && (result.ErrorType == "Revoked" || result.ErrorType == "Expired" || result.ErrorType == "Invalid"))
                return Unauthorized(new { message = result.Message });

            if (!result.Success)
                return BadRequest(new { message = result.Message });

           
            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = result.Message });
        }







        [AllowAnonymous]
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceInfo = Request.Headers["User-Agent"].ToString();

            var result = await _authService.GoogleSignInAsync(dto.IdToken, ipAddress, deviceInfo);

            if (!result.Success)
                return BadRequest(new { message = result.Message });


            var encodedToken = Uri.EscapeDataString(result.RefreshToken!);
            Response.Cookies.Append("refreshToken", encodedToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });

            return Ok(new
            {
                accessToken = result.Token,
                refreshToken = result.RefreshToken,
                message = result.Message,
                role = result.Role,
                isPlacementTestDone = result.IsPlacementTestDone,
                isGoalSet = result.IsGoalSet,
                isProfileCompleted = result.IsProfileCompleted
            });
        }
        [AllowAnonymous]
        [HttpPost("google-login-reviewer")]
        public async Task<IActionResult> GoogleLoginReviewer([FromBody] GoogleLoginRequestDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var deviceInfo = Request.Headers["User-Agent"].ToString();

            var result = await _authService.GoogleSignInReviewerAsync(dto.IdToken, ipAddress, deviceInfo);

            if (!result.Success)
                return BadRequest(new { message = result.Message });


            var encodedToken = Uri.EscapeDataString(result.RefreshToken!);
            Response.Cookies.Append("refreshToken", encodedToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(60)
            });


            return Ok(new
            {
                accessToken = result.Token,
                refreshToken = result.RefreshToken,
                message = result.Message,
                role = result.Role,
                isReviewerActive = result.IsReviewerActive
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetUserInfo()
        {
            // Lấy userId từ Claims của token (JWT token)
            var userIdClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            // Truy vấn thông tin người dùng từ database
            var user = await _userRepository.GetById(userId);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại." });
            }

            // Truy vấn các thông tin liên quan (ví dụ: profile)
            var learnerProfile = user.Role.ToUpper() == "LEARNER"
                                 ? await _learnerProfileRepository.GetByExpression(lp => lp.UserId == user.UserId)
                                 : null;

            var reviewerProfile = user.Role.ToUpper() == "REVIEWER"
                                 ? await _reviewerProfileRepository.GetByExpression(r => r.UserId == user.UserId)
                                 : null;

            // Cấu trúc dữ liệu trả về
            var userInfo = new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                user.Role,
                user.AvatarUrl,
                user.Status,
                LearnerProfile = learnerProfile,
                ReviewerProfile = reviewerProfile
            };

            return Ok(userInfo);
        }

    }


}
