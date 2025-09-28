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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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

            var result = await _authService.SignInAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                token = result.Token,
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
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            var result = await _authService.RefreshTokenAsync(dto);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(new
            {
                token = result.Token,
                message = result.Message,
                roleName = result.RoleName,
                isPlacementTestDone = result.IsPlacementTestDone,
                isGoalSet = result.IsGoalSet,
                isProfileCompleted = result.IsProfileCompleted
            });
        }


        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
        {
            var validationResult = ValidateModel();
            if (validationResult != null) return validationResult;

            await _authService.SendOtpAsync(dto.Email);
            return Ok(new { message = "OTP đã được gửi tới email." });
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

    }


}
