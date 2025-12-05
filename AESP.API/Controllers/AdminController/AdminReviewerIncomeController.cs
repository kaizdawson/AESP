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
    public class AdminReviewerIncomeController : ControllerBase
    {
        private readonly IAdminReviewerIncomeService _service;

        public AdminReviewerIncomeController(IAdminReviewerIncomeService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _service.GetSummaryAsync();
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetList([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _service.GetReviewerListAsync(search, pageNumber, pageSize, fromDate, toDate);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("reviewer-detail/{reviewerProfileId}")]
        public async Task<IActionResult> GetDetail(
            Guid reviewerProfileId,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var result = await _service.GetReviewerDetailAsync(reviewerProfileId, fromDate, toDate, pageNumber, pageSize);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}

