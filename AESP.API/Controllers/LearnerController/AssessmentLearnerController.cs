using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            // ✅ Lấy LearnerProfileId từ token thay vì query
            var learnerClaim = User.FindFirst("LearnerProfileId");
            if (learnerClaim == null)
                return Unauthorized(new { message = "Token không chứa LearnerProfileId" });

            Guid learnerProfileId = Guid.Parse(learnerClaim.Value);

            var response = await _assessmentService.GetPlacementTestForLearnerAsync(learnerProfileId);
            return Ok(response);
        }


    }
}
