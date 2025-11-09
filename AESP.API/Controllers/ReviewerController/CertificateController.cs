using AESP.Common.DTOs;
using AESP.Service.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AESP.API.Controllers.ReviewerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "REVIEWER")]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly IReviewerProfileService _reviewerProfileService;

        public CertificateController(ICertificateService certificateService, IReviewerProfileService reviewerProfileService)
        {
            _certificateService = certificateService;
            _reviewerProfileService = reviewerProfileService;
        }

        //  GET ALL — /api/reviewer/certificates/{profileId}
        [HttpGet("{reviewerProfileId}")]
        public async Task<IActionResult> GetCertificates(Guid reviewerProfileId)
        {
            var result = await _certificateService.GetByReviewerProfileIdAsync(reviewerProfileId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        //  UPLOAD FILE — /api/reviewer/certificates/upload/{profileId}
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCertificate(IFormFile file, [FromForm] string name)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(new { message = "Không xác định được người dùng từ token." });

            var reviewerProfile = await _reviewerProfileService.GetByUserIdAsync(Guid.Parse(userIdClaim));
            if (reviewerProfile == null)
                return NotFound(new { message = "Không tìm thấy hồ sơ reviewer." });

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File không hợp lệ." });

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Tên chứng chỉ không được để trống." });

            var result = await _certificateService.UploadCertificateAsync(reviewerProfile.ReviewerProfileId, file, name.Trim());
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        //  DELETE — /api/reviewer/certificates/{certificateId}
        [HttpDelete("{certificateId}")]
        public async Task<IActionResult> DeleteCertificate(Guid certificateId)
        {
            var result = await _certificateService.DeleteAsync(certificateId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
