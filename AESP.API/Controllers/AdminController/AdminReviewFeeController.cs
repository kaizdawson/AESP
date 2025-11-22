using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
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
        [HttpGet("review-fee-packages")]
        public async Task<IActionResult> GetAllReviewFeePackages(int pageNumber = 1, int pageSize = 10)
        {
            var result = await __adminReviewerFeeService.GetAllReviewFeePackagesAsync(pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 201 : 400, result);
        }
        [HttpGet("package/{reviewFeeId}")]
        public async Task<IActionResult> GetPackageDetail([FromRoute] Guid reviewFeeId)
        {
            try
            {
                var result = await __adminReviewerFeeService.GetReviewFeePackageDetailAsync(reviewFeeId);
                return result.IsSucess ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = BusinessCode.EXCEPTION,
                    Message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }
        [HttpGet("review-fee-packages/all")]
        [AllowAnonymous]  
        public async Task<IActionResult> GetAllReviewFeePackages()
        {
            var result = await __adminReviewerFeeService.GetAllReviewFeePackagesAsync();
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
