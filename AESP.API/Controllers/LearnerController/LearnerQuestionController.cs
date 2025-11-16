using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "LEARNER")]
public class LearnerQuestionController : ControllerBase
{
    private readonly ILearnerQuestionService _service;

    public LearnerQuestionController(ILearnerQuestionService service)
    {
        _service = service;
    }

    [HttpGet("exercise/{exerciseId}")]
    public async Task<IActionResult> GetQuestionsByExerciseId(Guid exerciseId)
    {
        // ✅ Lấy LearnerProfileId trực tiếp từ Claims
        var learnerProfileIdClaim = User.FindFirst("LearnerProfileId")?.Value;
        if (learnerProfileIdClaim == null)
            return Unauthorized(new { message = "Không tìm thấy LearnerProfileId trong token." });

        if (!Guid.TryParse(learnerProfileIdClaim, out Guid learnerProfileId))
            return BadRequest(new { message = "LearnerProfileId trong token không hợp lệ." });

        // ✅ Gọi service
        var result = await _service.GetQuestionsByExerciseIdForLearnerAsync(learnerProfileId, exerciseId);

        // ✅ Dùng helper anh đưa (chuẩn hóa HTTP code)
        return StatusFromResult(result);
    }

    // ✅ Helper: ánh xạ BusinessCode -> StatusCode
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