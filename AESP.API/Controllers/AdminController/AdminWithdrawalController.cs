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
    public class AdminWithdrawalController : ControllerBase
    {
        private readonly IAdminWithdrawalService _withdrawalService;

        public AdminWithdrawalController(IAdminWithdrawalService withdrawalService)
        {
            _withdrawalService = withdrawalService;
        }
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingWithdrawals(
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 10)
        {
            var result = await _withdrawalService.GetPendingWithdrawalsAsync(pageNumber, pageSize);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        // ================================
        // ADMIN DUYỆT RÚT TIỀN
        // ================================
        [HttpPut("approve/{transactionId}")]
        public async Task<IActionResult> ApproveWithdrawal(Guid transactionId)
        {
            var result = await _withdrawalService.ApproveWithdrawalAsync(transactionId);
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }

        // ================================
        // ADMIN TỪ CHỐI RÚT TIỀN
        // ================================
        [HttpPut("reject/{transactionId}")]
        public async Task<IActionResult> RejectWithdrawal(Guid transactionId, [FromBody] RejectReasonDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { Message = "Reason không được để trống." });

            var result = await _withdrawalService.RejectWithdrawalAsync(transactionId, dto.Reason.Trim());
            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
    }
}
