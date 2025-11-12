using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AESP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]

    public class LearningPathChapterController : ControllerBase
    {
        private readonly ILearningPathChapterService _service;

        public LearningPathChapterController(ILearningPathChapterService service)
        {
            _service = service;
        }

        // ============================================================
        // 🔹 Lấy tất cả chapter theo courseId trong LearningPath
        // ============================================================
        [HttpGet("by-course/{learningPathCourseId}")]
        public async Task<IActionResult> GetAllByCourse(Guid learningPathCourseId)
        {
            var result = await _service.GetAllByLearningPathCourseIdAsync(learningPathCourseId);
            return StatusFromResult(result);
        }

        // ============================================================
        // 🔹 Lấy chi tiết 1 chương học
        // ============================================================
        [HttpGet("{learningPathChapterId}")]
        public async Task<IActionResult> GetById(Guid learningPathChapterId)
        {
            var result = await _service.GetByIdAsync(learningPathChapterId);
            return StatusFromResult(result);
        }

        [HttpPost("by-course/{learningPathCourseId}")]
        public async Task<IActionResult> CreateByCourse(Guid learningPathCourseId, [FromBody] CreateLearningPathChapterRequestDTO dto)
        {
            var result = await _service.CreateByCourseAsync(learningPathCourseId, dto.LearnerCourseId);
            return StatusFromResult(result);
        }


        // ============================================================
        // 🔹 UPDATE PROGRESS
        // ============================================================
        [HttpPut("{learningPathChapterId}/progress")]
        public async Task<IActionResult> UpdateProgress(Guid learningPathChapterId, [FromBody] UpdateLearningPathChapterProgressDTO dto)
        {
            var result = await _service.UpdateProgressAsync(dto.LearnerCourseId, learningPathChapterId, dto.Progress);
            return StatusFromResult(result);
        }



        // ============================================================
        // 🔹 Helper (chuẩn logic bạn dùng toàn hệ thống)
        // ============================================================
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
