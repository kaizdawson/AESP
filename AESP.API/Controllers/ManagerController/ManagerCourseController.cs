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
        public async Task<IActionResult> CreateCourse([FromBody] CreateSimpleCourseDTO dto)
        {
            var fullDto = new CreateCourseFullDTO
            {
                Title = dto.Title,
                Type = dto.Type,
                NumberOfChapter = dto.NumberOfChapter,
                OrderIndex = dto.OrderIndex,
                Level = dto.Level,

                Price = dto.Price,
                Chapters = new List<CreateCourseChapterForCourseDTO>() // tạo course rỗng
            };

            var result = await _courseService.CreateFullCourseAsync(fullDto);
            return StatusFromResult(result);
        }



        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateSimpleCourseDTO dto)
        {
            var fullDto = new UpdateCourseFullDTO
            {
                Title = dto.Title,
                Type = dto.Type,
                NumberOfChapter = dto.NumberOfChapter,
                OrderIndex = dto.OrderIndex,
                Level = dto.Level,
                Chapters = dto.Chapters?.Select(ch => new UpdateCourseChapterForCourseDTO
                {
                    ChapterId = ch.ChapterId,
                    Title = ch.Title,
                    Description = ch.Description,
                    NumberOfExercise = ch.NumberOfExercise
                }).ToList()
            };

            var result = await _courseService.UpdateFullCourseAsync(id, fullDto);
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

            // Tùy theo BusinessCode mà chọn HTTP Code
            return result.BusinessCode switch
            {
                BusinessCode.VALIDATION_FAILED => BadRequest(result),
                BusinessCode.AUTH_NOT_FOUND => NotFound(result),
                BusinessCode.DATA_NOT_FOUND => BadRequest(result),

                BusinessCode.EXCEPTION => StatusCode(500, result),
                _ => Ok(result)
            };
        }
    }
}
