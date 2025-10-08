using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;

        public UploadController(ICloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }
        [HttpPost("image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] UploadFileDto dto)
        {
            try
            {
                var url = await _cloudinaryService.UploadImageAsync(dto.File, "AESP/images");
                return Ok(new { success = true, url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("video")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadVideo([FromForm] UploadFileDto dto)
        {
            try
            {
                var url = await _cloudinaryService.UploadVideoAsync(dto.File, "AESP/videos");
                return Ok(new { success = true, url });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
