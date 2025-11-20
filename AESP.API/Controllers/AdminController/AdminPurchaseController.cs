using AESP.Service.Contract;
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
    }
}
