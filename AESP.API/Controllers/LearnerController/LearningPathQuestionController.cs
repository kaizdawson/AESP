using AESP.Common.DTOs.BusinessCode;
using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class LearningPathQuestionController : ControllerBase
    {
        private readonly ILearningPathQuestionService _service;

        public LearningPathQuestionController(
            ILearningPathQuestionService service
        )
        {
            _service = service;
        }

        [HttpGet("not-started")]
        public async Task<IActionResult> GetNotStartedQuestions()
        {
            var result = await _service.GetAllNotStartedAsync();
            return HandleResult(result);
        }

        // ============================================================
        // 🔹 Helper xử lý Response (copy từ CourseLearnerController)
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
                    BusinessCode.DATA_NOT_FOUND => BadRequest(result),

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

            return Ok(result);
        }
    }
}
