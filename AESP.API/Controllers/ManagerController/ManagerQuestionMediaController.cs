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
    public class ManagerQuestionMediaController : ControllerBase
    {
        private readonly IQuestionMediaService _questionMediaService;

        public ManagerQuestionMediaController(IQuestionMediaService questionMediaService)
        {
            _questionMediaService = questionMediaService;
        }

        // ============================================================
        // 🔹 GET ALL (PHÂN TRANG)
        // ============================================================
        [HttpGet("medias")]
        public async Task<IActionResult> GetAllMedias(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? questionId = null)
        {
            var result = await _questionMediaService.GetAllQuestionMediasAsync(pageNumber, pageSize, questionId);
            return StatusFromResult(result);
        }

        // ============================================================
        // 🔹 GET BY ID
        // ============================================================
        [HttpGet("medias/{id}")]
        public async Task<IActionResult> GetMediaById(Guid id)
        {
            var result = await _questionMediaService.GetQuestionMediaByIdAsync(id);
            return StatusFromResult(result);
        }


        [HttpPost("medias/{questionId}")]
        public async Task<IActionResult> CreateMedia(Guid questionId, [FromBody] CreateQuestionMediaV2DTO dto)
        {
            var result = await _questionMediaService.CreateQuestionMediaAsync(questionId, dto);
            return StatusFromResult(result);
        }


        // ============================================================
        // 🔹 UPDATE
        // ============================================================
        [HttpPut("medias/{id}")]
        public async Task<IActionResult> UpdateMedia(Guid id, [FromBody] UpdateQuestionMediaV2DTO dto)
        {
            var result = await _questionMediaService.UpdateQuestionMediaAsync(id, dto);
            return StatusFromResult(result);
        }

        // ============================================================
        // 🔹 DELETE
        // ============================================================
        [HttpDelete("medias/{id}")]
        public async Task<IActionResult> DeleteMedia(Guid id)
        {
            var result = await _questionMediaService.DeleteQuestionMediaAsync(id);
            return StatusFromResult(result);
        }

        // ============================================================
        // 🔹 GET BY QUESTION ID (FULL LIST)
        // ============================================================
        [HttpGet("medias/question/{questionId}")]
        public async Task<IActionResult> GetMediasByQuestion(Guid questionId)
        {
            var result = await _questionMediaService.GetQuestionMediasByQuestionIdAsync(questionId);
            return StatusFromResult(result);
        }

        // ============================================================
        // 🔹 Helper: Auto map HTTP StatusCode từ BusinessCode
        // ============================================================
        private IActionResult StatusFromResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(500, new { message = "Không có phản hồi từ server." });

            return result.BusinessCode switch
            {
                // 400 – Dữ liệu không hợp lệ, trùng lặp, hành động sai
                BusinessCode.VALIDATION_FAILED or
                BusinessCode.VALIDATION_ERROR or
                BusinessCode.INVALID_INPUT or
                BusinessCode.INVALID_DATA or
                BusinessCode.INVALID_ACTION or
                BusinessCode.DUPLICATE_DATA
                    => BadRequest(result),

                // 401 – Không có quyền
                BusinessCode.AUTH_NOT_FOUND or BusinessCode.ACCESS_DENIED
                    => Unauthorized(result),

                // 404 – Không tìm thấy
                BusinessCode.DATA_NOT_FOUND
                    => NotFound(result),

                // 500 – Lỗi hệ thống
                BusinessCode.EXCEPTION or BusinessCode.INTERNAL_ERROR
                    => StatusCode(500, result),

                // 201 – Tạo thành công
                BusinessCode.INSERT_SUCESSFULLY or BusinessCode.CREATED_SUCCESSFULLY
                    => StatusCode(StatusCodes.Status201Created, result),

                // 200 – Thành công chung
                BusinessCode.GET_DATA_SUCCESSFULLY or
                BusinessCode.UPDATE_SUCESSFULLY or
                BusinessCode.DELETE_SUCESSFULLY
                    => Ok(result),

                // Mặc định
                _ => Ok(result)
            };
        }
    }
}
