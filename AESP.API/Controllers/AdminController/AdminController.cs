using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("toggle/{id}")]
        public async Task<IActionResult> ToggleUserStatus(Guid id)
        {
            var result = await _adminService.ToggleUserStatusAsync(id);
            if (result == "User not found")
                return NotFound(new { message = result });

            return Ok(new { message = result });
        }

        [HttpGet("learners")]
        public async Task<IActionResult> GetLearners()
        {
            var learners = await _adminService.GetLearnersAsync();
            return Ok(learners);
        }

        [HttpGet("mentors")]
        public async Task<IActionResult> GetMentors()
        {
            var mentors = await _adminService.GetMentorsAsync();
            return Ok(mentors);
        }
        [HttpPost("create-manager")]
        public async Task<IActionResult> CreateManager([FromBody] CreateManagerDto dto)
        {
            if (!ModelState.IsValid)
            {
                // Lấy lỗi đầu tiên thay vì tất cả
                var firstError = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault();

                return BadRequest(new { message = firstError ?? "Dữ liệu không hợp lệ." });
            }

            // ⚡ Chặn cả trường hợp Swagger để "string"
            if (string.IsNullOrWhiteSpace(dto.FullName) || dto.FullName == "string")
                return BadRequest(new { message = "Họ và tên không được để trống hoặc để giá trị mặc định." });

            if (string.IsNullOrWhiteSpace(dto.Email) || dto.Email == "string")
                return BadRequest(new { message = "Email không được để trống hoặc để giá trị mặc định." });

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber) || dto.PhoneNumber == "string")
                return BadRequest(new { message = "Số điện thoại không được để trống hoặc để giá trị mặc định." });

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password == "string")
                return BadRequest(new { message = "Mật khẩu không được để trống hoặc để giá trị mặc định." });

            var result = await _adminService.CreateManagerAsync(dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
