using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class CourseLearnerController : ControllerBase
    {
        
        private readonly ICourseService _courseService;
        private readonly ILearnerCourseService _learnerCourseService;

        public CourseLearnerController(ICourseService courseService, ILearnerCourseService learnerCourseService)
        {
            _courseService = courseService;
            _learnerCourseService = learnerCourseService;
        }

        //// ============================================================
        //// 🔹 Lấy danh sách khóa học theo Level hiện tại của Learner
        //// ============================================================
        //[HttpGet("level")]
        //public async Task<IActionResult> LearnerGetLevel(
        //    [FromQuery] string? level,
        //    [FromQuery] int pageNumber = 1,
        //    [FromQuery] int pageSize = 10,
        //    [FromQuery] string? keyword = null)
        //{
        //    var result = await _courseService.GetAllCourseAsync(pageNumber, pageSize, level, keyword);
        //    return Ok(result);
        //}



        [HttpGet("level/full")]
        public async Task<IActionResult> GetFullCoursesByLevel(
     [FromQuery] string level,
     [FromQuery] string? keyword = null)
        {
            var result = await _learnerCourseService.GetFullCoursesByLevelAsync(level, keyword);
            return HandleResult(result);
        }


        // ============================================================
        // 🔹 Xem chi tiết 1 khóa học (bao gồm Chapter / Exercise / Question)
        // ============================================================
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourseDetail(Guid courseId)
        {
            var result = await _courseService.GetFullCourseByIdAsync(courseId);
            return Ok(result);
        }

        [HttpPost("{courseId}/enroll")]
        public async Task<IActionResult> EnrollCourse(Guid courseId)
        {
            var learnerProfileIdClaim = User.Claims.FirstOrDefault(x => x.Type == "LearnerProfileId")?.Value;
            if (string.IsNullOrEmpty(learnerProfileIdClaim))
                return HandleResult(new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = BusinessCode.AUTH_NOT_FOUND,
                    Message = "Token không có LearnerProfileId."
                });

            Guid learnerProfileId = Guid.Parse(learnerProfileIdClaim);
            var result = await _learnerCourseService.EnrollAsync(learnerProfileId, courseId);
            return HandleResult(result);
        }


        [HttpPut("{courseId}/progress")]
        public async Task<IActionResult> UpdateProgress(Guid courseId, [FromBody] double progress)
        {
            Guid learnerId = Guid.Parse(User.Claims.First(x => x.Type == "sub").Value);
            var result = await _learnerCourseService.UpdateProgressAsync(learnerId, courseId, progress);
            return Ok(result);
        }

        [HttpDelete("{courseId}/unenroll")]
        public async Task<IActionResult> UnenrollCourse(Guid courseId)
        {
            Guid learnerId = Guid.Parse(User.Claims.First(x => x.Type == "sub").Value);
            var result = await _learnerCourseService.UnenrollAsync(learnerId, courseId);
            return Ok(result);
        }




        // ============================================================
        // 🔹 Helper xử lý Response chung cho controller này
        // ============================================================
        private IActionResult HandleResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(500, new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = BusinessCode.INTERNAL_ERROR,
                    Message = "Lỗi không xác định từ hệ thống."
                });

            if (!result.IsSucess)
            {
                return result.BusinessCode switch
                {
                    BusinessCode.DATA_NOT_FOUND => NotFound(result),

                    BusinessCode.VALIDATION_FAILED
                        or BusinessCode.VALIDATION_ERROR
                        or BusinessCode.INVALID_INPUT
                        or BusinessCode.INVALID_DATA
                        or BusinessCode.INVALID_ACTION => BadRequest(result),

                    BusinessCode.AUTH_NOT_FOUND
                        or BusinessCode.WRONG_PASSWORD => Unauthorized(result),

                    BusinessCode.ACCESS_DENIED
                        or BusinessCode.PERMISSION_DENIED => Forbid(),

                    BusinessCode.EXCEPTION
                        or BusinessCode.INTERNAL_ERROR => StatusCode(500, result),

                    _ => Ok(result)
                };
            }

            // ✅ Thành công
            return Ok(result);
        }


    }
}
