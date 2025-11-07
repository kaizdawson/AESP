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
