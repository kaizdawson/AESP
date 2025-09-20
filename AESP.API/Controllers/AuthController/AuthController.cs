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

        // --- SIGN UP ADMIN ---
        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] SignUpDto dto)
        {
            var result = await _authService.SignUpAsync(dto, 1);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // --- SIGN UP LEARNER ---
        [HttpPost("register/learner")]
        public async Task<IActionResult> RegisterLearner([FromBody] SignUpDto dto)
        {
            var result = await _authService.SignUpAsync(dto, 2);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // --- SIGN UP MENTOR ---
        [HttpPost("register/mentor")]
        public async Task<IActionResult> RegisterMentor([FromBody] SignUpDto dto)
        {
            var result = await _authService.SignUpAsync(dto, 3);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        // --- SIGN IN ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.SignInAsync(request);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { token = result.Token, message = result.Message });
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            var result = await _authService.RefreshTokenAsync(dto);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(new { token = result.Token, message = result.Message });
        }


        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromQuery] string email)
        {
            await _authService.SendOtpAsync(email);
            return Ok(new { message = "OTP đã được gửi tới email." });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyDto dto)
        {
            var result = await _authService.VerifyOtpAsync(dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }


        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
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
            var result = await _authService.ForgotPasswordAsync(dto);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [AllowAnonymous]
        [HttpPost("reset-password-by-link")]
        public async Task<IActionResult> ResetPasswordByLink([FromBody] ResetPasswordByLinkDto dto)
        {
            var result = await _authService.ResetPasswordByLinkAsync(dto);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

    }
}
