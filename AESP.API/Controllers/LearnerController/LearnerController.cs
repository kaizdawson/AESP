using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/learner/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;

        public AssessmentController(IAssessmentService assessmentService)
        {
            _assessmentService = assessmentService;
        }

        // ✅ 1. Learner tạo bài test (Start Test)
        [HttpPost("start")]
        public async Task<IActionResult> StartPlacementTest([FromBody] CreateAssessmentDTO dto)
        {
            var response = await _assessmentService.CreateAssessmentAsync(dto);
            return Ok(response);
        }

        // ✅ 2. Learner cập nhật bài test sau khi làm (Submit Result)
        [HttpPut("{id}")]
        public async Task<IActionResult> SubmitPlacementTest(Guid id, [FromBody] UpdateAssessmentDTO dto)
        {
            var response = await _assessmentService.UpdateAssessmentAsync(id, dto);
            return Ok(response);
        }

        // ✅ 3. Xem danh sách các bài test đã làm
        [HttpGet]
        public async Task<IActionResult> GetAllPlacementTests(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var learnerId = GetCurrentLearnerId(); // lấy từ JWT claim
            var response = await _assessmentService.GetAllAssessmentsAsync(pageNumber, pageSize, learnerId);
            return Ok(response);
        }

        // ✅ 4. Xem chi tiết bài test
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlacementTestById(Guid id)
        {
            var response = await _assessmentService.GetAssessmentByIdAsync(id);
            return Ok(response);
        }

        private Guid GetCurrentLearnerId()
        {
            var claim = User.FindFirst("UserId");
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }



    }
}
