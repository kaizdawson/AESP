using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    }
}
