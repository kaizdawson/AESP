using AESP.Repository.DB;
using AESP.Service.Contract;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AESP.API.Controllers.ReviewerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "REVIEWER")]
    public class ReviewerFeedbackController : ControllerBase
    {
        private readonly IReviewerFeedbackService _reviewerFeedbackService;
        private readonly AppDbContext _db;

        public ReviewerFeedbackController(
            IReviewerFeedbackService reviewerFeedbackService,
            AppDbContext db)
        {
            _reviewerFeedbackService = reviewerFeedbackService;
            _db = db;
        }
        [HttpGet("my-feedback")]
        public async Task<IActionResult> GetMyFeedback(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? feedbackType = null)
        {
            // 1) Lấy UserId từ token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Không xác định được UserId từ token." });

            // 2) Lấy ReviewerProfileId từ DB
            var reviewer = _db.ReviewerProfiles.FirstOrDefault(r => r.UserId == userId);
            if (reviewer == null)
                return Unauthorized(new { message = "Reviewer không tồn tại hoặc chưa kích hoạt." });

            // 3) Gọi service
            var result = await _reviewerFeedbackService.GetReviewerFeedbackAsync(
                reviewer.ReviewerProfileId, pageNumber, pageSize, feedbackType);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
