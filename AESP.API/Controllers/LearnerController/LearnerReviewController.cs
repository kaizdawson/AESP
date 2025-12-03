using AESP.Repository.DB;
using AESP.Repository.Models;
using AESP.Service.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class LearnerReviewController : ControllerBase
    {
        private readonly ILearnerBuyReview _service;

        public LearnerReviewController(ILearnerBuyReview service)
        {
            _service = service;
        }
        [HttpGet("my-history")]
        public async Task<IActionResult> GetLearnerReviewHistory(
   [FromQuery] int pageNumber = 1,
   [FromQuery] int pageSize = 10,
   [FromQuery] string? status = null,    
   [FromQuery] string? keyword = null,
   [FromQuery] string? feedbackType = null)
        {
            var learnerProfileId = GetLearnerProfileIdFromToken(User);

            if (learnerProfileId == null)
                return Unauthorized(new { message = "Không xác định được learner từ token." });

            var result = await _service.GetLearnerReviewHistoryAsync(
                learnerProfileId.Value, pageNumber, pageSize, status, keyword, feedbackType);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        private Guid? GetLearnerProfileIdFromToken(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return null;

            var db = HttpContext.RequestServices.GetService<AppDbContext>();
            var learner = db.Set<LearnerProfile>()
                            .FirstOrDefault(r => r.UserId == userId);

            return learner?.LearnerProfileId;
        }


    }
}
