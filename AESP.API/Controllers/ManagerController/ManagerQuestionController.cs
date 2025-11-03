using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.ManagerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MANAGER")]

    public class ManagerQuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public ManagerQuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        // ✅ GET ALL
        [HttpGet("questions")]
        public async Task<IActionResult> GetAllQuestions(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? exerciseId = null)
        {
            var result = await _questionService.GetAllQuestionsAsync(pageNumber, pageSize, exerciseId);
            return StatusFromResult(result);
        }

        // ✅ GET BY ID
        [HttpGet("questions/{id}")]
        public async Task<IActionResult> GetQuestionById(Guid id)
        {
            var result = await _questionService.GetQuestionByIdAsync(id);
            return StatusFromResult(result);
        }

        [HttpPost("questions/{exerciseId}")]
        public async Task<IActionResult> CreateQuestionsByExerciseId(Guid exerciseId, [FromBody] List<CreateQuestionDTO> dtos)
        {
            var result = await _questionService.CreateQuestionsByExerciseIdAsync(exerciseId, dtos);
            return StatusFromResult(result);
        }


        [HttpPut("questions/{id}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionDTO dto)
        {
            var result = await _questionService.UpdateQuestionAsync(id, dto);
            return StatusFromResult(result);
        }

        // ✅ DELETE
        [HttpDelete("questions/{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var result = await _questionService.DeleteQuestionAsync(id);
            return StatusFromResult(result);
        }

        // ✅ Helper: tự động chọn status code theo BusinessCode
        private IActionResult StatusFromResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(500, new { message = "Không có phản hồi từ server." });

            return result.BusinessCode switch
            {
                BusinessCode.VALIDATION_FAILED => BadRequest(result), // 400
                BusinessCode.DATA_NOT_FOUND => NotFound(result),      // 404
                BusinessCode.EXCEPTION => StatusCode(500, result),    // 500
                BusinessCode.INSERT_SUCESSFULLY => StatusCode(201, result), // 201
                _ => Ok(result) // 200
            };
        }
    }
}

