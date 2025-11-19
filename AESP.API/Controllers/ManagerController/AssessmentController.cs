using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AESP.API.Controllers.ManagerController
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]

    public class AssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;

        public AssessmentController(IAssessmentService assessmentService)
        {
            _assessmentService = assessmentService;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAllAssessments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? learnerId = null,
            [FromQuery] string? keyword = null)
        {
            var response = await _assessmentService.GetAllAssessmentsAsync(pageNumber, pageSize, learnerId, keyword);
            return Ok(response);
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssessmentById(Guid id)
        {
            var response = await _assessmentService.GetAssessmentByIdAsync(id);
            return Ok(response);
        }

        // ✅ CREATE
        [HttpPost]
        public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDTO dto)
        {
            var response = await _assessmentService.CreateAssessmentAsync(dto);
            return Ok(response);
        }

        // ✅ UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAssessment(Guid id, [FromBody] UpdateAssessmentDTO dto)
        {
            var response = await _assessmentService.UpdateAssessmentAsync(id, dto);
            return Ok(response);
        }

        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessment(Guid id)
        {
            var response = await _assessmentService.DeleteAssessmentAsync(id);
            return Ok(response);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAssessments(
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 10)
        {
            var result = await _assessmentService.GetAllAssessmentsAsync(pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
