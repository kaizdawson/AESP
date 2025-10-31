using AESP.Service.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.LearnerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class QuestionAssessmentLearnerController : ControllerBase
    {
        private readonly IQuestionAssessmentService _questionService;

        public QuestionAssessmentLearnerController(IQuestionAssessmentService questionService)
        {
            _questionService = questionService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
                                              [FromQuery] string? type = null, [FromQuery] string? keyword = null)
        {
            var response = await _questionService.GetAllQuestionAssessmentAsync(pageNumber, pageSize, type, keyword);
            return Ok(response);
        }
        [HttpGet("by-type")]
        public async Task<IActionResult> GetByType([FromQuery] string type)
        {
            var response = await _questionService.GetQuestionsByTypeAsync(type);
            return Ok(response);
        }
        [HttpGet("active/{id}")]
        public async Task<IActionResult> GetActiveQuestionById(Guid id)
        {
            var response = await _questionService.GetActiveQuestionByIdAsync(id);
            return Ok(response);
        }

        [HttpGet("by-type-active")]
        public async Task<IActionResult> GetActiveQuestionsByType([FromQuery] string type)
        {
            var response = await _questionService.GetActiveQuestionsByTypeAsync(type);
            return Ok(response);
        }


    }
}
