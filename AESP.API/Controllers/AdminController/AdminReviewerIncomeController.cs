using AESP.Service.Contract;
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

        //[HttpGet("summary")]
        //public async Task<IActionResult> GetSummary()
        //{
        //    var result = await _service.GetSummaryAsync();
        //    return StatusCode(result.IsSucess ? 200 : 400, result);
        //}

        //[HttpGet("list")]
        //public async Task<IActionResult> GetList([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        //{
        //    var result = await _service.GetReviewerListAsync(fromDate, toDate, search, pageNumber, pageSize);
        //    return StatusCode(result.IsSucess ? 200 : 400, result);
        //}

        //[HttpGet("{reviewerProfileId:guid}/detail")]
        //public async Task<IActionResult> GetReviewerDetail(Guid reviewerProfileId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        //{
        //    var result = await _service.GetReviewerDetailAsync(reviewerProfileId, fromDate, toDate);
        //    return StatusCode(result.IsSucess ? 200 : 400, result);
        //}
    }
}

