using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        //  GET danh sách user theo role
        [HttpGet("role/{roleName}")]
        public async Task<IActionResult> GetUsersByRole(string roleName)
        {
            var result = await _adminUserService.GetUsersByRoleAsync(roleName);
            return Ok(result);
        }

        //  GET chi tiết user theo ID
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserDetail(Guid userId)
        {
            var result = await _adminUserService.GetUserDetailAsync(userId);
            return Ok(result);
        }
    }
}
