using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

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
               
                var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
                if (userIdClaim == null)
                    return Unauthorized(new { message = "Token không chứa UserId." });

                Guid userId = Guid.Parse(userIdClaim.Value);

       
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
