using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminFeedbackController : ControllerBase
    {
        private readonly IAdminFeedbackService _service;

        public AdminFeedbackController(IAdminFeedbackService service)
        {
            _service = service;
        }

        //  Lấy danh sách Feedback
        [HttpGet]
        public async Task<IActionResult> GetAllFeedback()
        {
            var result = await _service.GetAllFeedbackAsync();
            return Ok(result);
        }

        //  Lấy chi tiết Feedback theo ID
        [HttpGet("{feedbackId}")]
        public async Task<IActionResult> GetFeedbackDetail(Guid feedbackId)
        {
            var result = await _service.GetFeedbackDetailAsync(feedbackId);
            return Ok(result);
        }
    }
}
