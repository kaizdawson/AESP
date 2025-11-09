using AESP.Common.DTOs;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminLearnerController : ControllerBase
    {
        private readonly IAdminLearnerService _adminLearnerService;

        public AdminLearnerController(IAdminLearnerService adminLearnerService)
        {
            _adminLearnerService = adminLearnerService;
        }
        //[HttpGet("list")]
        //public async Task<IActionResult> GetLearners(
        //    [FromQuery] string? search,
        //    [FromQuery] int pageNumber = 1,
        //    [FromQuery] int pageSize = 10,
        //    [FromQuery] string? filterStatus = "Active")
        //{
        //    var result = await _adminLearnerService.GetActiveLearnersAsync(search, pageNumber, pageSize, filterStatus);
        //    return StatusCode(result.IsSucess ? 200 : 400, result);
        //}
        [HttpPut("ban/{userId:guid}")]
        public async Task<IActionResult> BanLearner(Guid userId, [FromBody] BanReasonDTO body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.Reason))
                return BadRequest(new { Message = "Lý do chặn không được để trống." });

            var result = await _adminLearnerService.BanLearnerAsync(userId, body.Reason.Trim());
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        //[HttpGet("{learnerProfileId}/detail")]
        //public async Task<IActionResult> GetLearnerDetail(Guid learnerProfileId)
        //{
        //    var result = await _adminLearnerService.GetLearnerDetailAsync(learnerProfileId);
        //    return StatusCode(result.IsSucess ? 200 : 400, result);
        //}
    }
}

