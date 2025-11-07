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

    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;

        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExercises(
          [FromQuery] int pageNumber = 1,
          [FromQuery] int pageSize = 10,
          [FromQuery] Guid? chapterId = null,
          [FromQuery] string? keyword = null)
        {
            var result = await _exerciseService.GetAllExercisesAsync(pageNumber, pageSize, chapterId, keyword);
            return StatusFromResult(result);
        }

      
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExerciseById(Guid id)
        {
            var result = await _exerciseService.GetExerciseByIdAsync(id);
            return StatusFromResult(result);
        }


        // 🔹 CREATE (truyền chapterId qua path)
        // ============================================================
        [HttpPost("exercises/{chapterId}")]
        public async Task<IActionResult> CreateExercise(Guid chapterId, [FromBody] CreateExerciseDTO dto)
        {
            var result = await _exerciseService.CreateExerciseAsync(chapterId, dto);
            return StatusFromResult(result);
        }

        // ============================================================
        // 🔹 UPDATE
        // ============================================================
        [HttpPut("exercises/{id}")]
        public async Task<IActionResult> UpdateExercise(Guid id, [FromBody] UpdateExerciseDTO dto)
        {
            var result = await _exerciseService.UpdateExerciseAsync(id, dto);
            return StatusFromResult(result);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExercise(Guid id)
        {
            var result = await _exerciseService.DeleteExerciseAsync(id);
            return StatusFromResult(result);
        }



        [HttpGet("chapter/{chapterId}")]
        public async Task<IActionResult> GetExercisesByChapterId(Guid chapterId)
        {
            var result = await _exerciseService.GetExercisesByChapterIdAsync(chapterId);
            return StatusFromResult(result);
        }

        // Helper y như trên
        private IActionResult StatusFromResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(500, new { message = "Không có phản hồi từ server." });

            return result.BusinessCode switch
            {
                BusinessCode.VALIDATION_FAILED => BadRequest(result),
                BusinessCode.DATA_NOT_FOUND => NotFound(result),
                BusinessCode.EXCEPTION => StatusCode(500, result),
                BusinessCode.INSERT_SUCESSFULLY => StatusCode(201, result),
                _ => Ok(result)
            };
        }



    }
}
