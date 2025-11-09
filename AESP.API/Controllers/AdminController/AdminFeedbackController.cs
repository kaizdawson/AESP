using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] string? status = "all", [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetAllFeedbackAsync(keyword, status, pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        //  Lấy chi tiết Feedback theo ID
        [HttpGet("{feedbackId}")]
        public async Task<IActionResult> GetFeedbackDetail(Guid feedbackId)
        {
            var result = await _service.GetFeedbackDetailAsync(feedbackId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] string reason)
        {
            var res = await _service.RejectFeedbackAsync(id, reason);
            return StatusCode(res.IsSucess ? 200 : 400, res);
        }
    }
}
