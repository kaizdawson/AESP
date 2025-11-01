using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        //[HttpGet("summary")]
        //public async Task<IActionResult> GetSummary()
        //{
        //    var result = await _dashboardService.GetSummaryAsync();
        //    return Ok(result);
        //}

        //[HttpGet("packages")]
        //public async Task<IActionResult> GetPackagesByMonth([FromQuery] int year)
        //{
        //    var result = await _dashboardService.GetPackagesByMonthAsync(year);
        //    return Ok(result);
        //}

        //[HttpGet("revenue")]
        //public async Task<IActionResult> GetRevenueByMonth([FromQuery] int year)
        //{
        //    var result = await _dashboardService.GetRevenueByMonthAsync(year);
        //    return Ok(result);
        //}

        //  Reviewer đang chờ duyệt
        [HttpGet("reviewers/pending")]
        public async Task<IActionResult> GetPendingReviewers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _dashboardService.GetPendingReviewersAsync(pageNumber, pageSize);
            return Ok(result);
        }
    }
}
