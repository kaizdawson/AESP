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

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class LearnerAnswerController : ControllerBase
    {
        private readonly ILearnerAnswerService _learnerAnswerService;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepo;

        public LearnerAnswerController(
            ILearnerAnswerService learnerAnswerService,
            IGenericRepository<LearnerProfile> learnerProfileRepo)
        {
            _learnerAnswerService = learnerAnswerService;
            _learnerProfileRepo = learnerProfileRepo;
        }




        [HttpPost("{questionId}/submit")]
        public async Task<IActionResult> SubmitAnswer(Guid questionId, [FromBody] SubmitLearnerAnswerDTO dto)
        {
            try
            {
                Guid learnerProfileId;

                var learnerProfileIdClaim = User.Claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
                if (learnerProfileIdClaim != null && Guid.TryParse(learnerProfileIdClaim.Value, out var parsedLearnerId))
                {
                    learnerProfileId = parsedLearnerId;
                }
                else
                {
                    var userIdClaim = User.Claims.FirstOrDefault(c =>
                        c.Type == JwtRegisteredClaimNames.Sub ||
                        c.Type == ClaimTypes.NameIdentifier ||
                        c.Type.EndsWith("/nameidentifier")
                    );

                    if (userIdClaim == null)
                        return Unauthorized(new { message = "Token không chứa UserId hoặc LearnerProfileId." });

                    if (!Guid.TryParse(userIdClaim.Value, out var userId))
                        return Unauthorized(new { message = "UserId trong token không hợp lệ." });

                    var learnerProfile = await _learnerProfileRepo.AsQueryable()
                        .FirstOrDefaultAsync(lp => lp.UserId == userId);

                    if (learnerProfile == null)
                        return Unauthorized(new { message = "Không tìm thấy hồ sơ học viên (LearnerProfile)." });

                    learnerProfileId = learnerProfile.LearnerProfileId;
                }

                // Gọi service – truyền questionId riêng
                var result = await _learnerAnswerService.SubmitAnswerAsync(learnerProfileId, questionId, dto);

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }




        // ============================================================
        // 🔹 Helper xử lý Response chung cho controller này
        // ============================================================
        private IActionResult HandleResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(500, new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = BusinessCode.INTERNAL_ERROR,
                    Message = "Lỗi không xác định từ hệ thống."
                });

            if (!result.IsSucess)
            {
                return result.BusinessCode switch
                {
                    BusinessCode.DATA_NOT_FOUND => NotFound(result),

                    BusinessCode.VALIDATION_FAILED
                        or BusinessCode.VALIDATION_ERROR
                        or BusinessCode.INVALID_INPUT
                        or BusinessCode.INVALID_DATA
                        or BusinessCode.INVALID_ACTION => BadRequest(result),

                    BusinessCode.AUTH_NOT_FOUND
                        or BusinessCode.WRONG_PASSWORD => Unauthorized(result),

                    BusinessCode.ACCESS_DENIED
                        or BusinessCode.PERMISSION_DENIED => Forbid(),

                    BusinessCode.EXCEPTION
                        or BusinessCode.INTERNAL_ERROR => StatusCode(500, result),

                    _ => Ok(result)
                };
            }

            // ✅ Thành công
            return Ok(result);
        }
    }
}
