using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminReviewerController : ControllerBase
    {
        private readonly IAdminReviewerService _adminReviewerService;

        public AdminReviewerController(IAdminReviewerService adminReviewerService)
        {
            _adminReviewerService = adminReviewerService;
        }

        //  Lấy danh sách reviewer chờ duyệt
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingReviewers([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            var result = await _adminReviewerService.GetPendingReviewersAsync(pageNumber, pageSize);
            return Ok(result);
        }

        //  Duyệt reviewer
        [HttpPut("{reviewerProfileId}/approve")]
        public async Task<IActionResult> ApproveReviewer(Guid reviewerProfileId)
        {
            var result = await _adminReviewerService.ApproveReviewerAsync(reviewerProfileId);
            return Ok(result);
        }

        //  Từ chối reviewer
        [HttpPut("{reviewerProfileId}/reject")]
        public async Task<IActionResult> RejectReviewer(Guid reviewerProfileId)
        {
            var result = await _adminReviewerService.RejectReviewerAsync(reviewerProfileId);
            return Ok(result);
        }
    }
}
