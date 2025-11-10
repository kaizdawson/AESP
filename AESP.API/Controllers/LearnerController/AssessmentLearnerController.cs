using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]

    public class AssessmentLearnerController : ControllerBase
    {

        private readonly IAssessmentService _assessmentService;

        public AssessmentLearnerController(IAssessmentService assessmentService)
        {
            _assessmentService = assessmentService;
        }



        [HttpGet("placement-test")]
        public async Task<IActionResult> GetPlacementTestForLearner()
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == JwtRegisteredClaimNames.Sub ||
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type.EndsWith("/nameidentifier"));
                if (userIdClaim == null)
                    return Unauthorized(new { message = "Token không chứa UserId." });

                Guid userId = Guid.Parse(userIdClaim.Value);

                // ⚙️ Nếu token có IsPlacementTestDone = true thì chặn
                var placementClaim = User.Claims.FirstOrDefault(c => c.Type == "IsPlacementTestDone");
                bool isPlacementTestDone = placementClaim != null && bool.TryParse(placementClaim.Value, out var done) && done;

                if (isPlacementTestDone)
                    return BadRequest(new { message = "Learner đã hoàn thành bài test đầu vào, không thể làm lại." });

                // ✅ Gọi service
                var response = await _assessmentService.GetPlacementTestForLearnerAsync(userId);
                if (!response.IsSucess)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("placement-test/submit")]
        public async Task<IActionResult> SubmitPlacementTest([FromBody] CreatePlacementTestDTO dto)
        {
            var result = await _assessmentService.SubmitPlacementTestCombinedAsync(dto);
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
