using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]

    public class LearningPathExerciseController : ControllerBase
    {
        private readonly ILearningPathExerciseService _service;

        public LearningPathExerciseController(ILearningPathExerciseService service)
        {
            _service = service;
        }

        // ============================================================
        // 🔹 GET: /api/LearningPathExercise/chapter/{learningPathChapterId}
        // ============================================================
        [HttpGet("chapter/{learningPathChapterId}")]
        public async Task<IActionResult> GetByLearningPathChapterId(Guid learningPathChapterId)
        {
            var result = await _service.GetByLearningPathChapterIdAsync(learningPathChapterId);
            return StatusFromResult(result);
        }


        // ============================================================
        // 🔹 Helper chuẩn map BusinessCode -> HTTP Code
        // ============================================================
        private IActionResult StatusFromResult(ResponseDTO result)
        {
            if (result == null)
                return StatusCode(500, new { message = "Không có phản hồi từ server." });

            return result.BusinessCode switch
            {
                BusinessCode.VALIDATION_FAILED or
                BusinessCode.VALIDATION_ERROR or
                BusinessCode.INVALID_INPUT or
                BusinessCode.INVALID_DATA or
                BusinessCode.INVALID_ACTION or
                BusinessCode.DUPLICATE_DATA
                    => BadRequest(result),

                BusinessCode.AUTH_NOT_FOUND or BusinessCode.ACCESS_DENIED
                    => Unauthorized(result),

                BusinessCode.DATA_NOT_FOUND
                    => NotFound(result),

                BusinessCode.EXCEPTION or BusinessCode.INTERNAL_ERROR
                    => StatusCode(500, result),

                BusinessCode.INSERT_SUCESSFULLY or BusinessCode.CREATED_SUCCESSFULLY
                    => StatusCode(StatusCodes.Status201Created, result),

                BusinessCode.GET_DATA_SUCCESSFULLY or
                BusinessCode.UPDATE_SUCESSFULLY or
                BusinessCode.DELETE_SUCESSFULLY
                    => Ok(result),

                _ => Ok(result)
            };
        }
    }
}
