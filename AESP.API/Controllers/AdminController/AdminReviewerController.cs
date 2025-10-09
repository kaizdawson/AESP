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
        [HttpPut("approve/{certificateId}")]
        public async Task<IActionResult> ApproveReviewer(Guid certificateId)
        {
            var result = await _adminReviewerService.ApproveReviewerByCertificateAsync(certificateId);
            return Ok(result);
        }

        //  Từ chối reviewer
        [HttpPut("reject/{certificateId}")]
        public async Task<IActionResult> RejectReviewer(Guid certificateId)
        {
            var result = await _adminReviewerService.RejectReviewerByCertificateAsync(certificateId);
            return Ok(result);
        }
    }
}
