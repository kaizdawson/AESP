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
            var result = await _courseService.GetFullCourseByIdAsync(id);
            return StatusFromResult(result);
        }

        //// ✅ CREATE
        //[HttpPost("courses")]
        //public async Task<IActionResult> CreateCourse([FromBody] CreateSimpleCourseDTO dto)
        //{
        //    var fullDto = new CreateCourseFullDTO
        //    {
        //        Title = dto.Title,
        //        Type = dto.Type,
        //        NumberOfChapter = dto.NumberOfChapter,
        //        OrderIndex = dto.OrderIndex,
        //        Level = dto.Level,
        //        Chapters = dto.Chapters?.Select(ch => new CreateCourseChapterForCourseDTO
        //        {
        //            Title = ch.Title,
        //            Description = ch.Description,
        //            NumberOfExercise = ch.NumberOfExercise
        //        }).ToList()
        //    };

        //    var result = await _courseService.CreateFullCourseAsync(fullDto);
        //    return StatusFromResult(result);
        //}


        [HttpPost("courses")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseFullDTO dto)
        {
            var result = await _courseService.CreateFullCourseAsync(dto);
            return StatusFromResult(result);
        }



        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateSimpleCourseDTO dto)
        {
            var result = await _courseService.UpdateCourseAsync(id, dto);
            return StatusFromResult(result);
        }



        // ✅ DELETE
        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var result = await _courseService.DeleteFullCourseAsync(id);
            return StatusFromResult(result);
        }

        // ✅ GET COURSES BY LEVEL
        [HttpGet("courses/level/{level}")]
        public async Task<IActionResult> GetCoursesByLevel(string level)
        {
            var result = await _courseService.GetCoursesByLevelAsync(level);
            return StatusFromResult(result);
        }



        // ✅ Helper để tự động map status code đúng
        private IActionResult StatusFromResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(500, new { message = "Không có phản hồi từ server." });

            return result.BusinessCode switch
            {
                // ❌ 400 – Dữ liệu không hợp lệ, trùng lặp, hành động sai, đầu vào sai
                BusinessCode.VALIDATION_FAILED or
                BusinessCode.VALIDATION_ERROR or
                BusinessCode.INVALID_INPUT or
                BusinessCode.INVALID_DATA or
                BusinessCode.INVALID_ACTION or
                BusinessCode.DUPLICATE_DATA
                    => BadRequest(result),

                // 🚫 401 – Không có quyền hoặc chưa xác thực
                BusinessCode.AUTH_NOT_FOUND or BusinessCode.ACCESS_DENIED
                    => Unauthorized(result),

                // 🔍 404 – Không tìm thấy
                BusinessCode.DATA_NOT_FOUND
                    => NotFound(result),

                // 💥 500 – Lỗi hệ thống
                BusinessCode.EXCEPTION or BusinessCode.INTERNAL_ERROR
                    => StatusCode(500, result),

                // ✅ 201 – Tạo thành công
                BusinessCode.INSERT_SUCESSFULLY or BusinessCode.CREATED_SUCCESSFULLY
                    => StatusCode(StatusCodes.Status201Created, result),

                // ✅ 200 – Cập nhật, xóa, lấy dữ liệu thành công
                BusinessCode.GET_DATA_SUCCESSFULLY or
                BusinessCode.UPDATE_SUCESSFULLY or
                BusinessCode.DELETE_SUCESSFULLY
                    => Ok(result),

                // Mặc định 200 OK nếu không match case nào
                _ => Ok(result)
            };
        }

    }
}
