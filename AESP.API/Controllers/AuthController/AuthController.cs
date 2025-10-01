using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AESP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }
        private IActionResult? ValidateModel()
        {
            if (!ModelState.IsValid)
            {
                var errors = new List<string>();

                foreach (var entry in ModelState)
                {
                    var fieldErrors = entry.Value.Errors.Select(e => e.ErrorMessage).ToList();
                    if (fieldErrors.Any(e => e.Contains("không được để trống")))
                    {
                        errors.Add(fieldErrors.First(e => e.Contains("không được để trống")));
                    }
                    else
                    {
                        errors.AddRange(fieldErrors);
                    }
                }

                return BadRequest(new { messages = errors });
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

            return Ok(new { message = result.Message });
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

            return Ok(new
            {
                accessToken = result.Token,
                refreshToken = result.RefreshToken,
                message = result.Message,
                roleName = result.RoleName,
                isPlacementTestDone = result.IsPlacementTestDone,
                isGoalSet = result.IsGoalSet,
                isProfileCompleted = result.IsProfileCompleted
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
                roleName = result.RoleName
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

            var result = await _authService.ResetPasswordByLinkAsync(dto);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
        {
            var result = await _authService.LogoutAsync(dto.RefreshToken);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        [AllowAnonymous]
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var redirectUri = "https://localhost:7017/api/auth/google-callback"; 
            var scope = "openid profile email";

            var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
                      $"?response_type=code" +
                      $"&client_id={clientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&scope={Uri.EscapeDataString(scope)}";

            return Ok(new { url });
        }

        [AllowAnonymous]
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code)
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var clientSecret = _configuration["Authentication:Google:ClientSecret"];
            var redirectUri = "https://localhost:7017/api/auth/google-callback";

            using var http = new HttpClient();

            var tokenRequest = new Dictionary<string, string>
    {
        { "code", code },
        { "client_id", clientId! },
        { "client_secret", clientSecret! },
        { "redirect_uri", redirectUri },
        { "grant_type", "authorization_code" }
    };

            var response = await http.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(tokenRequest));
            var json = await response.Content.ReadAsStringAsync();

            var tokenData = System.Text.Json.JsonDocument.Parse(json).RootElement;
            var idToken = tokenData.GetProperty("id_token").GetString();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(idToken);

            var email = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var avatar = jwt.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(email))
                return BadRequest(new { message = "Không lấy được email từ Google" });

           
            var result = await _authService.SignInWithGoogleAsync(email, name ?? "", avatar ?? "");

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }


    }


}
