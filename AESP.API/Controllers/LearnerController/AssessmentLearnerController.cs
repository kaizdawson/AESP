using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]

    public class AssessmentLearnerController : ControllerBase
    {

        private readonly IAssessmentService _assessmentService;

        public AssessmentLearnerController(IAssessmentService assessmentService)
        {
            _assessmentService = assessmentService;
        }



        [HttpGet("placement-test")]
        public async Task<IActionResult> GetPlacementTestForLearner()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == JwtRegisteredClaimNames.Sub ||
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type.EndsWith("/nameidentifier"));
                if (userIdClaim == null)
                    return Unauthorized(new { message = "Token không chứa UserId." });

                Guid userId = Guid.Parse(userIdClaim.Value);

                // ⚙️ Nếu token có IsPlacementTestDone = true thì chặn
                var placementClaim = User.Claims.FirstOrDefault(c => c.Type == "IsPlacementTestDone");
                bool isPlacementTestDone = placementClaim != null && bool.TryParse(placementClaim.Value, out var done) && done;

                if (isPlacementTestDone)
                    return BadRequest(new { message = "Learner đã hoàn thành bài test đầu vào, không thể làm lại." });

                // ✅ Gọi service
                var response = await _assessmentService.GetPlacementTestForLearnerAsync(userId);
                if (!response.IsSucess)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }



    }
}
