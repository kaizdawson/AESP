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

    public class ManagerQuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;

        public ManagerQuestionController(IQuestionService questionService)
        {
            _questionService = questionService;
        }

        // ✅ GET ALL
        [HttpGet("questions")]
        public async Task<IActionResult> GetAllQuestions(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? exerciseId = null)
        {
            var result = await _questionService.GetAllQuestionsAsync(pageNumber, pageSize, exerciseId);
            return StatusFromResult(result);
        }

        // ✅ GET BY ID
        [HttpGet("questions/{id}")]
        public async Task<IActionResult> GetQuestionById(Guid id)
        {
            var result = await _questionService.GetQuestionByIdAsync(id);
            return StatusFromResult(result);
        }

        [HttpPost("questions/{exerciseId}")]
        public async Task<IActionResult> CreateQuestionsByExerciseId(Guid exerciseId, [FromBody] List<CreateQuestionDTO> dtos)
        {
            var result = await _questionService.CreateQuestionsByExerciseIdAsync(exerciseId, dtos);
            return StatusFromResult(result);
        }

        [HttpPut("questions/{id}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionDTO dto)
        {
            var result = await _questionService.UpdateQuestionAsync(id, dto);
            return StatusFromResult(result);
        }


        // ✅ DELETE
        [HttpDelete("questions/{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            var result = await _questionService.DeleteQuestionAsync(id);
            return StatusFromResult(result);
        }



        // ✅ GET LIST BY EXERCISE ID (chuẩn 3 lớp, dùng StatusFromResult)
        [HttpGet("questions/exercise/{exerciseId}")]
        public async Task<IActionResult> GetQuestionsByExerciseId(Guid exerciseId)
        {
            var result = await _questionService.GetQuestionsByExerciseIdAsync(exerciseId);
            return StatusFromResult(result);
        }


        // ✅ Helper: tự động chọn status code theo BusinessCode
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

