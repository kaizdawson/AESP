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
        [HttpGet("summary")]
        public async Task<IActionResult> GetWithdrawalSummary()
        {
            var result = await _withdrawalService.GetWithdrawalSummaryAsync();

            if (!result.IsSucess)
                return BadRequest(result);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllWithdrawals(
           [FromQuery] string? keyword = null,
           [FromQuery] string? status = "all",
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 10
       )
        {
            var result = await _withdrawalService.GetAllWithdrawalAsync(
                keyword,
                status,
                pageNumber,
                pageSize
            );

            if (!result.IsSucess)
                return BadRequest(result);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }
        [HttpGet("transfer-transactions")]
        public async Task<IActionResult> GetAllTransferTransactions(
            [FromQuery] string? keyword = null,
            [FromQuery] string? type = null,           // ReviewPayment / ReviewerTip
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                // GỌI HÀM BẠN VỪA ĐƯỢC MÌNH VIẾT Ở SERVICE
                var result = await _withdrawalService.GetAllTransferTransactionsAsync(
                    keyword: keyword,
                    type: type,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                return result.IsSucess
                    ? Ok(result)
                    : BadRequest(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new ResponseDTO
                {
                    IsSucess = false,
                    BusinessCode = AESP.Common.DTOs.BusinessCode.BusinessCode.EXCEPTION,
                    Message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }
    }
}
