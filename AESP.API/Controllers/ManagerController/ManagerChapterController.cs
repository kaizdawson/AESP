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

    public class ManagerChapterController : ControllerBase
    {
        private readonly IChapterService _chapterService;

        public ManagerChapterController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        // ✅ GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAllChapters(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? courseId = null,
            [FromQuery] string? keyword = null)
        {
            var response = await _chapterService.GetAllChaptersAsync(pageNumber, pageSize, courseId, keyword);
            return Ok(response);
        }

        // ✅ GET BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetChapterById(Guid id)
        {
            var response = await _chapterService.GetChapterByIdAsync(id);
            return Ok(response);
        }

        // ✅ CREATE
        [HttpPost("chapters")]
        public async Task<IActionResult> CreateChapter([FromBody] CreateSimpleChapterDTO dto)
        {
            var fullDto = new CreateChapterDTO
            {
                Title = dto.Title,
                Description = dto.Description,
                NumberOfExercise = dto.NumberOfExercise,
                CourseId = dto.CourseId,
                Exercises = dto.Exercises?.Select(e => new CreateChapterExerciseDTO
                {
                    Title = e.Title,
                    Description = e.Description,
                    OrderIndex = e.OrderIndex,
                    NumberOfQuestion = e.NumberOfQuestion,
                    Questions = new List<CreateChapterQuestionDTO>() // không cần gửi
                }).ToList()
            };

            var result = await _chapterService.CreateChapterAsync(fullDto);
            return StatusFromResult(result);
        }

        [HttpPut("chapters/{id}")]
        public async Task<IActionResult> UpdateChapter(Guid id, [FromBody] UpdateSimpleChapterDTO dto)
        {
            var fullDto = new UpdateChapterDTO
            {
                Title = dto.Title ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                NumberOfExercise = dto.NumberOfExercise,
                Exercises = dto.Exercises?.Select(e => new UpdateChapterExerciseDTO
                {
                    ExerciseId = e.ExerciseId,
                    Title = e.Title ?? string.Empty,
                    Description = e.Description ?? string.Empty,
                    OrderIndex = e.OrderIndex,
                    NumberOfQuestion = e.NumberOfQuestion,
                    Questions = new List<UpdateChapterQuestionDTO>() // placeholder
                }).ToList()
            };

            var result = await _chapterService.UpdateChapterAsync(id, fullDto);
            return StatusFromResult(result);
        }


        // ✅ DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChapter(Guid id)
        {
            var response = await _chapterService.DeleteChapterAsync(id);
            return Ok(response);
        }



        // ✅ Helper để tự động map status code đúng
        private IActionResult StatusFromResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(500, new { message = "Không có phản hồi từ server." });

            // Tùy theo BusinessCode mà chọn HTTP Code
            return result.BusinessCode switch
            {
                BusinessCode.VALIDATION_FAILED => BadRequest(result),
                BusinessCode.AUTH_NOT_FOUND => NotFound(result),
                BusinessCode.EXCEPTION => StatusCode(500, result),
                _ => Ok(result)
            };
        }
    }
}

