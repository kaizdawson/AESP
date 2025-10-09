using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.ManagerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "MANAGER")]
    public class ManagerCourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public ManagerCourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // ✅ GET ALL
        [HttpGet("courses")]
        public async Task<IActionResult> GetAllCourses(
            int pageNumber = 1,
            int pageSize = 10,
            string? level = null,
            string? keyword = null)
        {
            var result = await _courseService.GetAllCourseAsync(pageNumber, pageSize, level, keyword);
            return StatusFromResult(result);
        }

        // ✅ GET BY ID
        [HttpGet("courses/{id}")]
        public async Task<IActionResult> GetCourseById(Guid id)
        {
            var result = await _courseService.GetByCourseIdAsync(id);
            return StatusFromResult(result);
        }

        // ✅ CREATE
        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDTO dto)
        {
            if (!ModelState.IsValid)
            {
                // Nếu enum bị sai -> message sẽ có lỗi parse ở đây
                var errorMessages = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    isSucess = false,
                    businessCode = BusinessCode.VALIDATION_FAILED,
                    message = "Cấp độ (Level) không hợp lệ. Giá trị hợp lệ: A1, A2, B1, B2, C1, C2."
                });
            }

            var result = await _courseService.CreateCourseAsync(dto);
            return Ok(result);
        }



        // ✅ UPDATE
        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseDTO dto)
        {
            var result = await _courseService.UpdateCourseAsync(id, dto);
            return StatusFromResult(result);
        }

        // ✅ DELETE
        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var result = await _courseService.DeleteCourseAsync(id);
            return StatusFromResult(result);
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
