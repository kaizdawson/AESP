using AESP.Common.DTOs;
using AESP.Repository.DB;
using AESP.Repository.Models;
using AESP.Service.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AESP.API.Controllers.ReviewerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "REVIEWER")]
    public class ReviewerReviewController : ControllerBase
    {
        private readonly IReviewerReviewService _reviewService;
        private readonly IReviewerProfileService _reviewerProfileService;

        public ReviewerReviewController(
            IReviewerReviewService reviewService,
            IReviewerProfileService reviewerProfileService)
        {
            _reviewService = reviewService;
            _reviewerProfileService = reviewerProfileService;
        }
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = Common.DTOs.BusinessCode.BusinessCode.INVALID_INPUT,
                    Message = "Dữ liệu gửi lên không hợp lệ."
                });
            }

            var result = await _reviewService.SubmitReviewAsync(
                dto.ReviewerProfileId,
                dto.LearnerAnswerId,
                dto.RecordId,
                dto.Score,
                dto.Comment
            );

            // ✅ Format trả về chuẩn dự án (StatusCode 200/400)
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("history")]
        public async Task<IActionResult> GetReviewHistory(
    [FromQuery][Required] Guid reviewerProfileId,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var reviewerProfileId = GetReviewerProfileIdFromToken(User);
            if (reviewerProfileId == null)
                return Unauthorized(new { message = "Không xác định được reviewer từ token." });

            var result = await _reviewService.GetReviewHistoryAsync(
                reviewerProfileId.Value, pageNumber, pageSize);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingReviews(
            [FromQuery][Required] Guid reviewerProfileId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var reviewerProfileId = GetReviewerProfileIdFromToken(User);
            if (reviewerProfileId == null)
                return Unauthorized(new { message = "Không xác định được reviewer từ token." });

            var result = await _reviewService.GetPendingReviewsAsync(
                reviewerProfileId.Value, pageNumber, pageSize);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        private Guid? GetReviewerProfileIdFromToken(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              user.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return null;

            var db = HttpContext.RequestServices.GetService<AppDbContext>();
            // nếu service KHÔNG expose DbContext thì sửa bên dưới
            var reviewer = db.Set<ReviewerProfile>()
                             .FirstOrDefault(r => r.UserId == userId);

            return reviewer?.ReviewerProfileId;
        }


    }

   
}

