using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "LEARNER")]
public class LearnerQuestionController : ControllerBase
{
    private readonly ILearnerQuestionService _service;
    private readonly IGenericRepository<LearnerProfile> _learnerProfileRepo;

    public LearnerQuestionController(ILearnerQuestionService service, IGenericRepository<LearnerProfile> learnerProfileRepo)
    {
        _service = service;
        _learnerProfileRepo = learnerProfileRepo;
    }

    [HttpGet("exercise/{exerciseId}")]
    public async Task<IActionResult> GetQuestionsByExerciseId(Guid exerciseId)
    {
        try
        {
            Guid learnerProfileId;

            
            var learnerProfileIdClaim = User.Claims.FirstOrDefault(c => c.Type == "LearnerProfileId");
            if (learnerProfileIdClaim != null && Guid.TryParse(learnerProfileIdClaim.Value, out var parsedLearnerId))
            {
                learnerProfileId = parsedLearnerId;
            }
            else
            {
                var userIdClaim = User.Claims.FirstOrDefault(c =>
                    c.Type == JwtRegisteredClaimNames.Sub ||
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type.EndsWith("/nameidentifier"));

                if (userIdClaim == null)
                    return Unauthorized(new { message = "Token không chứa UserId hoặc LearnerProfileId." });

                Guid userId = Guid.Parse(userIdClaim.Value);

                var learnerProfile = await _learnerProfileRepo.AsQueryable()
                    .FirstOrDefaultAsync(lp => lp.UserId == userId);

                if (learnerProfile == null)
                    return Unauthorized(new { message = "Không tìm thấy hồ sơ học viên (LearnerProfile)." });

                learnerProfileId = learnerProfile.LearnerProfileId;
            }

            var result = await _service.GetQuestionsByExerciseIdForLearnerAsync(
                learnerProfileId,
                exerciseId
            );

            return StatusFromResult(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
        }
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