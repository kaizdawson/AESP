using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.ReviewerController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "REVIEWER")]
    public class CertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;

        public CertificateController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        //  GET ALL — /api/reviewer/certificates/{profileId}
        [HttpGet("{reviewerProfileId}")]
        public async Task<IActionResult> GetCertificates(Guid reviewerProfileId)
        {
            var result = await _certificateService.GetByReviewerProfileIdAsync(reviewerProfileId);
            return Ok(result);
        }

        //  UPLOAD FILE — /api/reviewer/certificates/upload/{profileId}
        [HttpPost("upload/{reviewerProfileId}")]
        public async Task<IActionResult> UploadCertificate(Guid reviewerProfileId, IFormFile file, string name)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File không hợp lệ." });

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Tên chứng chỉ không được để trống." });

            var result = await _certificateService.UploadCertificateAsync(reviewerProfileId, file, name.Trim());
            return Ok(result);
        }

        //  DELETE — /api/reviewer/certificates/{certificateId}
        [HttpDelete("{certificateId}")]
        public async Task<IActionResult> DeleteCertificate(Guid certificateId)
        {
            var result = await _certificateService.DeleteAsync(certificateId);
            return Ok(result);
        }
    }
}
