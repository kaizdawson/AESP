using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "LEARNER")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackDTO dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    isSucess = false,
                    message = "Dữ liệu không hợp lệ.",

                });


            var result = await _feedbackService.AddFeedbackAsync(dto);

            return Ok(result);
        }
    }

}
