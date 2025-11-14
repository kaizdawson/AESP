using AESP.Common.DTOs;
using AESP.Service.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AESP.API.Controllers.ReviewerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "REVIEWER")]
    public class ReviewerReviewController : ControllerBase
    {
        private readonly IReviewerReviewService _reviewService;

        public ReviewerReviewController(IReviewerReviewService reviewService)
        {
            _reviewService = reviewService;
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
            var result = await _reviewService.GetReviewHistoryAsync(reviewerProfileId, pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingReviews(
            [FromQuery][Required] Guid reviewerProfileId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _reviewService.GetPendingReviewsAsync(reviewerProfileId, pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

    }

   
}

