using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.ManagerController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerQuestionAssessmentController : ControllerBase
    {
        private readonly IQuestionAssessmentService _service;

        public ManagerQuestionAssessmentController(IQuestionAssessmentService service)
        {
            _service = service;
        }

        [HttpGet("questions")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10, string? type = null, string? keyword = null)
        {
            var result = await _service.GetAllAsync(pageNumber, pageSize, type, keyword);
            return Ok(result);
        }

        [HttpGet("questions/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost("questions")]
        public async Task<IActionResult> Create([FromBody] CreateQuestionAssessmentDTO dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }

        [HttpPut("questions/{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuestionAssessmentDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("questions/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            return Ok(result);
        }
    }
}
