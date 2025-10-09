using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.ReviewerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "REVIEWER")]
    public class ReviewerProfileController : ControllerBase
    {
        private readonly IReviewerProfileService _reviewerProfileService;

        public ReviewerProfileController(IReviewerProfileService reviewerProfileService)
        {
            _reviewerProfileService = reviewerProfileService;
        }

        // ✅ GET PROFILE
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(Guid userId)
        {
            var result = await _reviewerProfileService.GetByUserIdAsync(userId);
            return Ok(result);
        }

        // ✅ UPDATE PROFILE
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateReviewerProfile(Guid userId, [FromBody] UpdateReviewerProfileDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ.", errors = ModelState });

            var result = await _reviewerProfileService.UpdateProfileAsync(userId, dto);
            return Ok(result);
        }
    }
}
