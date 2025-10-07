using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.ManagerController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerCourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public ManagerCourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // ✅ GET ALL — /api/manager/courses?pageNumber=1&pageSize=10&level=A2&keyword=speaking
        [HttpGet("courses")]
        public async Task<IActionResult> GetAllCourses(
            int pageNumber = 1,
            int pageSize = 10,
            string? level = null,
            string? keyword = null)
        {
            var result = await _courseService.GetAllAsync(pageNumber, pageSize, level, keyword);
            return Ok(result);
        }

        // ✅ GET BY ID — /api/manager/courses/{id}
        [HttpGet("courses/{id}")]
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            var result = await _courseService.GetByIdAsync(id);
            return Ok(result);
        }

        // ✅ CREATE — /api/manager/courses
        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid course data." });

            var result = await _courseService.CreateAsync(dto);
            return Ok(result);
        }

        // ✅ UPDATE — /api/manager/courses/{id}
        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDTO dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid course data." });

            var result = await _courseService.UpdateAsync(id, dto);
            return Ok(result);
        }

        // ✅ DELETE — /api/manager/courses/{id}
        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var result = await _courseService.DeleteAsync(id);
            return Ok(result);
        }
    }
}
