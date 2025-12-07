using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using AESP.Service.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AESP.API.Controllers.AdminController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminPurchaseController : ControllerBase
    {
        private readonly IAdminPurchaseService _service;

        public AdminPurchaseController(IAdminPurchaseService service)
        {
            _service = service;
        }
        [HttpGet("list")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] string? type = null   // course | reviewfee | aiconversation
        )
        {
            var result = await _service.GetAllAsync(pageNumber, pageSize, keyword, type);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("detail/{purchaseId:guid}")]
        public async Task<IActionResult> GetDetail(Guid purchaseId)
        {
            var result = await _service.GetDetailAsync(purchaseId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportPdf()
        {
            var bytes = await _service.ExportPdfAsync();
            return File(bytes, "application/pdf", "purchase-report.pdf");
        }
        [HttpGet("purchases/dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var result = await _service.GetDashboardAsync();

                return StatusCode(result.IsSucess
                    ? StatusCodes.Status200OK
                    : StatusCodes.Status400BadRequest, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = BusinessCode.EXCEPTION,
                    Message = ex.Message
                });
            }
        }
        [HttpGet("reviewfee-buyers")]
        public async Task<IActionResult> GetReviewFeeBuyers(
    int pageNumber = 1,
    int pageSize = 10)
        {
            var result = await _service.GetReviewFeeBuyerStatisticsAsync(pageNumber, pageSize);
            return Ok(result);
        }
        [HttpGet("ai-buyers")]
        public async Task<IActionResult> GetAIConversationBuyers(
    int pageNumber = 1,
    int pageSize = 10)
        {
            var result = await _service.GetAIConversationBuyerStatisticsAsync(pageNumber, pageSize);
            return Ok(result);
        }
        [HttpGet("course-enroll")]
        public async Task<IActionResult> GetEnrolledCourseStatistics(
     int pageNumber = 1,
     int pageSize = 10)
        {
            var result = await _service.GetEnrolledCourseStatisticsAsync(pageNumber, pageSize);
            return Ok(result);
        }
    }
}
