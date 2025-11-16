using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AESP.API.Controllers.Learner
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class LearnerReviewRequestController : ControllerBase
    {
        private readonly ILearnerReviewRequestService _reviewService;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepo;

        public LearnerReviewRequestController(
            ILearnerReviewRequestService reviewService,
            IGenericRepository<LearnerProfile> learnerProfileRepo)
        {
            _reviewService = reviewService;
            _learnerProfileRepo = learnerProfileRepo;
        }

        // ==========================================================
        // 🔹 PRIVATE: Lấy LearnerProfileId từ token (có fallback)
        // ==========================================================
        private async Task<Guid?> ResolveLearnerProfileIdAsync()
        {
            // ---- 1) Lấy từ LearnerProfileId claim (nếu có)
            var learnerProfileIdClaim = User.Claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
            if (learnerProfileIdClaim != null && Guid.TryParse(learnerProfileIdClaim.Value, out var parsedLearnerId))
            {
                return parsedLearnerId;
            }

            // ---- 2) Lấy UserId từ token nếu không có LearnerProfileId
            var userIdClaim = User.Claims.FirstOrDefault(c =>
                c.Type == JwtRegisteredClaimNames.Sub ||
                c.Type == ClaimTypes.NameIdentifier ||
                c.Type.EndsWith("/nameidentifier")
            );

            if (userIdClaim == null)
                return null;

            if (!Guid.TryParse(userIdClaim.Value, out Guid userId))
                return null;

            // ---- 3) Fallback: lấy LearnerProfile theo UserId
            var learnerProfile = await _learnerProfileRepo.AsQueryable()
                .FirstOrDefaultAsync(lp => lp.UserId == userId);

            return learnerProfile?.LearnerProfileId;
        }

        // ==========================================================
        // 1) Learner bật / tắt yêu cầu review
        [HttpPut("toggle/{answerId}")]
        public async Task<IActionResult> ToggleReviewRequest(
      Guid answerId,
      [FromBody] ToggleReviewRequestDTO dto)
        {
            try
            {
                Guid learnerProfileId;

                // ============================
                // 1) Lấy LearnerProfileId từ token (nếu có)
                // ============================
                var learnerProfileIdClaim = User.Claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
                if (learnerProfileIdClaim != null && Guid.TryParse(learnerProfileIdClaim.Value, out var parsedLearnerId))
                {
                    learnerProfileId = parsedLearnerId;
                }
                else
                {
                    // ============================
                    // 2) Fallback lấy UserId từ token
                    // ============================
                    var userIdClaim = User.Claims.FirstOrDefault(c =>
                        c.Type == JwtRegisteredClaimNames.Sub ||
                        c.Type == ClaimTypes.NameIdentifier ||
                        c.Type.EndsWith("/nameidentifier")
                    );

                    if (userIdClaim == null)
                        return Unauthorized(new { message = "Token không chứa UserId hoặc LearnerProfileId." });

                    Guid userId = Guid.Parse(userIdClaim.Value);

                    // ============================
                    // 3) Lấy LearnerProfile theo UserId
                    // ============================
                    var learnerProfile = await _learnerProfileRepo.AsQueryable()
                        .FirstOrDefaultAsync(lp => lp.UserId == userId);

                    if (learnerProfile == null)
                        return Unauthorized(new { message = "Không tìm thấy hồ sơ học viên." });

                    learnerProfileId = learnerProfile.LearnerProfileId;
                }

                // ============================
                // 4) Gọi service
                // ============================
                var result = await _reviewService.UpdateReviewFlagAsync(
                    learnerProfileId,
                    answerId,
                    dto.IsNeededReview,
                    dto.NumberOfReview
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }



        // ==========================================================
        // 2) Lấy danh sách câu trả lời learner đang yêu cầu review
        // ==========================================================
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyReviewRequests()
        {
            var learnerProfileId = await ResolveLearnerProfileIdAsync();
            if (!learnerProfileId.HasValue)
                return Unauthorized(new { message = "Không tìm thấy LearnerProfileId trong token." });

            var result = await _reviewService.GetMyReviewRequestsAsync(learnerProfileId.Value);
            return Ok(result);
        }

        // ==========================================================
        // 3) Xóa yêu cầu review của 1 answer
        // ==========================================================
        [HttpDelete("{answerId}")]
        public async Task<IActionResult> ClearReviewRequest(Guid answerId)
        {
            var learnerProfileId = await ResolveLearnerProfileIdAsync();
            if (!learnerProfileId.HasValue)
                return Unauthorized(new { message = "Không tìm thấy LearnerProfileId trong token." });

            var result = await _reviewService.ClearReviewRequestAsync(learnerProfileId.Value, answerId);
            return Ok(result);
        }
    }

    // DTO
    public class ToggleReviewFlagRequest
    {
        public Guid AnswerId { get; set; }
        public bool IsNeededReview { get; set; }
    }
}
