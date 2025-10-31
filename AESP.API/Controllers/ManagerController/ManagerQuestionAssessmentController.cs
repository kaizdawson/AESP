using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.ManagerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MANAGER")]

    public class ManagerQuestionAssessmentController : ControllerBase
    {
        private readonly IQuestionAssessmentService _service;

        public ManagerQuestionAssessmentController(IQuestionAssessmentService service)
        {
            _service = service;
        }

        // ✅ GET ALL — /api/ManagerQuestionAssessment/questions
        [HttpGet("questions")]
        public async Task<IActionResult> GetAll(
            int pageNumber = 1,
            int pageSize = 10,
            string? type = null,
            string? keyword = null)
        {
            var result = await _service.GetAllQuestionAssessmentAsync(pageNumber, pageSize, type, keyword);
            return StatusFromResult(result);
        }

        // ✅ GET BY ID — /api/ManagerQuestionAssessment/questions/{id}
        [HttpGet("questions/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByQuestionAssessmentIdAsync(id);
            return StatusFromResult(result);
        }

        // ✅ CREATE — /api/ManagerQuestionAssessment/questions
        [HttpPost("questions")]
        public async Task<IActionResult> Create([FromBody] CreateQuestionAssessmentDTO dto)
        {
            var result = await _service.CreateQuestionAssessmentAsync(dto);
            return StatusFromResult(result);
        }

        // ✅ UPDATE — /api/ManagerQuestionAssessment/questions/{id}
        [HttpPut("questions/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuestionAssessmentDTO dto)
        {
            var result = await _service.UpdateQuestionAssessmentAsync(id, dto);
            return StatusFromResult(result);
        }

        // ✅ DELETE — /api/ManagerQuestionAssessment/questions/{id}
        [HttpDelete("questions/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteQuestionAssessmentAsync(id);
            return StatusFromResult(result);
        }
        [HttpPut("reset-status/{type}")]
        public async Task<IActionResult> ResetStatusByType(string type)
        {
            var response = await _service.ResetStatusByTypeAsync(type);
            return Ok(response);
        }

        // ✅ Helper method để ánh xạ BusinessCode → HTTP Code
        private IActionResult StatusFromResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Không có phản hồi từ server." });

            return result.BusinessCode switch
            {
                BusinessCode.VALIDATION_FAILED => BadRequest(result),            // 400
                BusinessCode.AUTH_NOT_FOUND => NotFound(result),                // 404
                BusinessCode.EXCEPTION => StatusCode(StatusCodes.Status500InternalServerError, result), // 500
                _ => Ok(result)                                                 // 200
            };
        }



        [HttpPut("set-status/{id}")]
        [Authorize(Roles = "MANAGER,ADMIN")]
        public async Task<IActionResult> SetQuestionStatus(Guid id, [FromQuery] bool status)
        {
            var response = await _service.SetQuestionStatusAsync(id, status);
            return Ok(response);
        }

    }
}
