using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AESP.API.Controllers.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AvatarController : ControllerBase
    {
        private readonly IAvatarService _avatarService;

        public AvatarController(IAvatarService avatarService)
        {
            _avatarService = avatarService;
        }
        [HttpPost("avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarFileDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(new { message = "Không xác định được người dùng từ token." });

            var result = await _avatarService.UploadAvatarAsync(Guid.Parse(userIdClaim), dto.File);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
