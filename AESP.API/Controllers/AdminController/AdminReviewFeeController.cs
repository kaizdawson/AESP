using AESP.Common.DTOs;
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
    public class AdminReviewFeeController : ControllerBase
    {
        private readonly IAdminReviewFeeService __adminReviewerFeeService;

        public AdminReviewFeeController(IAdminReviewFeeService adminReviewerFeeService)
        {
            __adminReviewerFeeService = adminReviewerFeeService;
        }
        [HttpPost("review-fee-package")]
        public async Task<IActionResult> CreateReviewFeePackage([FromBody] CreateReviewFeePackageDto dto)
        {
            var result = await __adminReviewerFeeService.CreateReviewFeePackageAndDetailAsync(dto);
            return StatusCode(result.IsSucess ? 201 : 400, result);
        }
        [HttpPost("review-fee-policy")]
        public async Task<IActionResult> ScheduleNewReviewFeePolicy([FromBody] UpdateReviewFeeDetailDto dto)
        {
            var result = await __adminReviewerFeeService.ScheduleNewReviewFeeDetailAsync(dto);
            return StatusCode(result.IsSucess ? 201 : 400, result);
        }
    }
}
