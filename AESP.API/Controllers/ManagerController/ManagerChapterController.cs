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

        // ============================================================
        // 🔹 CREATE (truyền courseId qua path)
        // ============================================================
        [HttpPost("chapters/{courseId}")]
        public async Task<IActionResult> CreateChapter(Guid courseId, [FromBody] CreateSimpleChapterDTO dto)
        {
            var fullDto = new CreateChapterDTO
            {
                Title = dto.Title,
                Description = dto.Description,
                NumberOfExercise = dto.NumberOfExercise,
            };

            var result = await _chapterService.CreateChapterAsync(courseId, fullDto);
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
                CourseId = dto.CourseId
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


        // ✅ GET CHAPTERS BY COURSE ID (chuẩn 3 lớp, dùng StatusFromResult)
        [HttpGet("chapters/course/{courseId}")]
        public async Task<IActionResult> GetChaptersByCourse(Guid courseId)
        {
            var result = await _chapterService.GetChaptersByCourseIdAsync(courseId);
            return StatusFromResult(result);
        }




        // ✅ Helper để tự động map status code đúng
        private IActionResult StatusFromResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(500, new { message = "Không có phản hồi từ server." });

            return result.BusinessCode switch
            {
                BusinessCode.VALIDATION_FAILED => BadRequest(result),  // 400
                BusinessCode.DATA_NOT_FOUND => NotFound(result),       // 404
                BusinessCode.EXCEPTION => StatusCode(500, result),     // 500
                BusinessCode.INSERT_SUCESSFULLY => StatusCode(201, result), // 201 Created
                _ => Ok(result) // 200 OK
            };
        }
    }
}

