using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AESP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    isSucess = false,
                    message = "Dữ liệu không hợp lệ.",
                });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            var result = await _feedbackService.AddFeedbackAsync(dto, userId);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }

}
