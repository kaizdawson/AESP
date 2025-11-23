using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class ProgressAnalyticsController : ControllerBase
    {
        private readonly IProgressAnalyticsQueryService _progressService;

        public ProgressAnalyticsController(IProgressAnalyticsQueryService progressService)
        {
            _progressService = progressService;
        }

        // GET api/progressanalytics/my
        [HttpGet("my")]
        public async Task<IActionResult> GetMyProgress()
        {
            // sub trong token chính là UserId (như bạn đã cấu hình JwtService)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                               ?? User.FindFirst(ClaimTypes.Name)
                               ?? User.FindFirst("sub");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Không đọc được UserId từ token.");

            var result = await _progressService.GetMyProgressAsync(userId);
            if (!result.IsSucess)
                return BadRequest(result);

            return Ok(result);
        }

        // Nếu admin muốn xem theo learnerProfileId
        // GET api/progressanalytics/by-learner/{learnerProfileId}
        [HttpGet("by-learner/{learnerProfileId:guid}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> GetByLearner(Guid learnerProfileId)
        {
            var result = await _progressService.GetByLearnerProfileIdAsync(learnerProfileId);
            if (!result.IsSucess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
