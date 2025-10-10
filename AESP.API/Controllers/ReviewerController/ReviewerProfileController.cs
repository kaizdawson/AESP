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
        public async Task<IActionResult> UpdateReviewerProfile(Guid userId, [FromBody] ReviewerProfileUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                // ⚡ Lấy lỗi đầu tiên theo thứ tự property trong DTO
                var orderedFields = new[] { "Experience", "FullName", "PhoneNumber" };

                var firstError = ModelState
                    .Where(ms => ms.Value.Errors.Any())
                    .OrderBy(ms => Array.IndexOf(orderedFields, ms.Key.Split('.').Last()))
                    .SelectMany(ms => ms.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                // ✅ Trả message duy nhất (ngắn gọn, đúng lỗi đầu tiên)
                return BadRequest(new { message = firstError });
            }

            var result = await _reviewerProfileService.UpdateProfileAsync(userId, dto);
            return Ok(result);
        }
    }
}
