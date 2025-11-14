using AESP.Common.DTOs;
using AESP.Service.Contract;
using AESP.Service.Implementation;
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
        public async Task<IActionResult> GetPendingReviewers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _adminReviewerService.GetPendingReviewersAsync(pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        //  Duyệt reviewer
        [HttpPut("approve/{certificateId}")]
        public async Task<IActionResult> ApproveReviewer(Guid certificateId)
        {
            var result = await _adminReviewerService.ApproveReviewerByCertificateAsync(certificateId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        //  Từ chối reviewer
        [HttpPut("reject/{certificateId}")]
        public async Task<IActionResult> RejectReviewer(Guid certificateId)
        {
            var result = await _adminReviewerService.RejectReviewerByCertificateAsync(certificateId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveReviewers(
   [FromQuery] string? search,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? filterStatus = "Actived")
        {
            var result = await _adminReviewerService.GetActiveReviewersAsync(search, pageNumber, pageSize, filterStatus);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("{reviewerProfileId}/detail")]
        public async Task<IActionResult> GetReviewerDetail(Guid reviewerProfileId)
        {
            var result = await _adminReviewerService.GetReviewerDetailAsync(reviewerProfileId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpPut("ban/{userId}")]
        public async Task<IActionResult> BanReviewer(Guid userId, [FromBody] BanReasonDTO body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Reason))
                return BadRequest(new { Message = "Lý do chặn không được để trống." });

            var result = await _adminReviewerService.BanReviewerAsync(userId, body.Reason.Trim());
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpPut("level/{reviewerProfileId}")]
        public async Task<IActionResult> UpdateReviewerLevel(Guid reviewerProfileId, [FromBody] UpdateReviewerLevelDto dto)
        {
            var result = await _adminReviewerService.UpdateReviewerLevelAsync(reviewerProfileId, dto.Level);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("{reviewerProfileId}/pending-certificates")]
        public async Task<IActionResult> GetReviewerPendingCert(Guid reviewerProfileId)
        {
            var result = await _adminReviewerService.GetUnapprovedCertificatesAsync(reviewerProfileId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("certificate/pending")]
        public async Task<IActionResult> GetPendingCertificates(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var result = await _adminReviewerService.GetAllPendingCertificatesAsync(pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
