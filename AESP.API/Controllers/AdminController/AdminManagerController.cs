using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminManagerController : ControllerBase
    {
        private readonly IAdminManagerService _adminManagerService;

        public AdminManagerController(IAdminManagerService adminManagerService)
        {
            _adminManagerService = adminManagerService;
        }

        // 🔹 Danh sách Manager (phân trang + tìm kiếm)
        [HttpGet("list")]
        public async Task<IActionResult> GetManagers(
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _adminManagerService.GetManagersAsync(search, pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        // 🔹 Xem chi tiết Manager
        [HttpGet("{userId:guid}/detail")]
        public async Task<IActionResult> GetManagerDetail(Guid userId)
        {
            var result = await _adminManagerService.GetManagerDetailAsync(userId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
