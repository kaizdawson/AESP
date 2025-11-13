using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class CourseLearnerController : ControllerBase
    {

        private readonly ICourseService _courseService;
        private readonly ILearnerCourseService _learnerCourseService;
        private readonly IGenericRepository<LearnerProfile> _learnerProfileRepo;

        public CourseLearnerController(
            ICourseService courseService,
            ILearnerCourseService learnerCourseService,
            IGenericRepository<LearnerProfile> learnerProfileRepo)
        {
            _courseService = courseService;
            _learnerCourseService = learnerCourseService;
            _learnerProfileRepo = learnerProfileRepo;
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
            try
            {
                // 1️⃣ Lấy LearnerProfileId từ token
                var learnerProfileIdClaim = User.Claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
                Guid learnerProfileId = Guid.Empty;

                if (learnerProfileIdClaim != null && Guid.TryParse(learnerProfileIdClaim.Value, out var parsedLearnerId))
                {
                    learnerProfileId = parsedLearnerId;
                }
                else
                {
                    // 2️⃣ Fallback: Lấy UserId nếu token thiếu LearnerProfileId
                    var userIdClaim = User.Claims.FirstOrDefault(c =>
                        c.Type == JwtRegisteredClaimNames.Sub ||
                        c.Type == ClaimTypes.NameIdentifier ||
                        c.Type.EndsWith("/nameidentifier"));

                    if (userIdClaim == null)
                        return Unauthorized(new { message = "Token không chứa UserId hoặc LearnerProfileId." });

                    Guid userId = Guid.Parse(userIdClaim.Value);

                    // 3️⃣ Tìm LearnerProfile bằng UserId
                    var learnerProfile = await _learnerProfileRepo.AsQueryable()
                        .FirstOrDefaultAsync(lp => lp.UserId == userId);

                    if (learnerProfile == null)
                        return Unauthorized(new { message = "Không tìm thấy hồ sơ học viên (LearnerProfile)." });

                    learnerProfileId = learnerProfile.LearnerProfileId;
                }

                // 4️⃣ Gọi service enroll
                var result = await _learnerCourseService.EnrollAsync(learnerProfileId, courseId);

                if (!result.IsSucess)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
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
            return HandleResult(result);
        }



        [HttpPost("relearn-exercise")]
        public async Task<IActionResult> RelearnExercise([FromBody] RelearnExerciseRequestDTO dto)
        {
            try
            {
                Guid learnerProfileId;

                // 1️⃣ Lấy LearnerProfileId từ token
                var learnerProfileIdClaim = User.Claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
                if (learnerProfileIdClaim != null && Guid.TryParse(learnerProfileIdClaim.Value, out var parsedLearnerId))
                {
                    learnerProfileId = parsedLearnerId;
                }
                else
                {
                    // 2️⃣ Fallback: lấy UserId
                    var userIdClaim = User.Claims.FirstOrDefault(c =>
                        c.Type == JwtRegisteredClaimNames.Sub ||
                        c.Type == ClaimTypes.NameIdentifier ||
                        c.Type.EndsWith("/nameidentifier"));

                    if (userIdClaim == null)
                        return Unauthorized(new { message = "Token không chứa UserId hoặc LearnerProfileId." });

                    Guid userId = Guid.Parse(userIdClaim.Value);

                    // 3️⃣ Tìm LearnerProfile
                    var learnerProfile = await _learnerProfileRepo.AsQueryable()
                        .FirstOrDefaultAsync(lp => lp.UserId == userId);

                    if (learnerProfile == null)
                        return Unauthorized(new { message = "Không tìm thấy hồ sơ học viên (LearnerProfile)." });

                    learnerProfileId = learnerProfile.LearnerProfileId;
                }

                // 4️⃣ Gọi service
                var result = await _learnerCourseService.RelearnAndUpdateScoreAsync(
                    learnerProfileId,
                    dto.ExerciseId,
                    dto.NewScore
                );

                return HandleResult(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }







        [HttpGet("myLevels")]
        public async Task<IActionResult> GetMyLevels()
        {
            try
            {
                Guid learnerProfileId;

                // 1️⃣ Lấy LearnerProfileId từ token (nếu có)
                var learnerProfileIdClaim = User.Claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
                if (learnerProfileIdClaim != null && Guid.TryParse(learnerProfileIdClaim.Value, out var parsedLearnerId))
                {
                    learnerProfileId = parsedLearnerId;
                }
                else
                {
                    // 2️⃣ Fallback lấy UserId từ token
                    var userIdClaim = User.Claims.FirstOrDefault(c =>
                        c.Type == JwtRegisteredClaimNames.Sub ||
                        c.Type == ClaimTypes.NameIdentifier ||
                        c.Type.EndsWith("/nameidentifier")
                    );

                    if (userIdClaim == null)
                        return Unauthorized(new { message = "Token không chứa UserId hoặc LearnerProfileId." });

                    Guid userId = Guid.Parse(userIdClaim.Value);

                    // 3️⃣ Tìm LearnerProfile theo UserId
                    var learnerProfile = await _learnerProfileRepo.AsQueryable()
                        .FirstOrDefaultAsync(lp => lp.UserId == userId);

                    if (learnerProfile == null)
                        return Unauthorized(new { message = "Không tìm thấy hồ sơ học viên (LearnerProfile)." });

                    learnerProfileId = learnerProfile.LearnerProfileId;
                }

                // 4️⃣ Gọi service
                var result = await _learnerCourseService.GetMyLevelsAsync(learnerProfileId);
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // DTO request body
        public class RelearnExerciseRequestDTO
        {
            public Guid ExerciseId { get; set; }
            public double? NewScore { get; set; } // null = bắt đầu học lại, có giá trị = cập nhật điểm
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
