using AESP.Common.DTOs;
using AESP.Common.DTOs.BusinessCode;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AESP.API.Controllers.CoinController
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoinController : ControllerBase
    {
        private readonly ICoinService _coinService;

        public CoinController(ICoinService coinService)
        {
            _coinService = coinService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetMyCoinBalance()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Access token không hợp lệ hoặc thiếu UserId.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("UserId trong token không hợp lệ.");

            try
            {
                var balance = await _coinService.GetUserCoinBalanceAsync(userId);
                return Ok(new { coinBalance = balance });
            }
            catch
            {
                return BadRequest(new { message = "Không thể lấy thông tin coin balance." });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCoin([FromBody] AddCoinRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Access token không hợp lệ hoặc thiếu UserId.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("UserId trong token không hợp lệ.");

            try
            {
                var result = await _coinService.AddCoinAsync(request.ServicePackageId, userId);
                return Ok(result);

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelTransaction([FromBody] CancelTransactionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OrderCode))
                return BadRequest(new { message = "Thiếu orderCode." });

            try
            {
                await _coinService.CancelTransactionByOrderCodeAsync(request.OrderCode);
                return Ok(new { message = "Giao dịch đã hủy thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("status/{orderCode}")]
        public async Task<IActionResult> GetTransactionStatus(string orderCode)
        {
            try
            {
                var status = await _coinService.GetTransactionStatusAsync(orderCode);
                return Ok(new { orderCode, status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("pay")]
        public async Task<IActionResult> PayCoin([FromBody] PayCoinRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Access token không hợp lệ hoặc thiếu UserId.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("UserId trong token không hợp lệ.");

            try
            {
                var result = await _coinService.PayCoinAsync(userId, request.AIConversationChargeId);

                if (result == 1)
                    return Ok(new { result = 1, message = "Thanh toán thành công." });

                return BadRequest(new { result = 0, message = "Số dư trong ví của bạn không đủ." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpPost("withdraw")]
        public async Task<IActionResult> WithdrawCoin([FromBody] WithdrawRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Access token không hợp lệ hoặc thiếu UserId.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("UserId trong token không hợp lệ.");

            try
            {
                var result = await _coinService.WithdrawCoinAsync(
                    userId,
                    request.Coin,
                    request.BankName,
                    request.AccountNumber
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("history/deposit")]
        public async Task<IActionResult> GetDepositHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Token không hợp lệ.");

            try
            {
                var result = await _coinService.GetDepositHistoryAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("history/withdraw")]
        public async Task<IActionResult> GetWithdrawHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Token không hợp lệ.");

            try
            {
                var result = await _coinService.GetWithdrawHistoryAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("ai-packages")]
        public async Task<IActionResult> GetActiveAIPackages()
        {
            try
            {
                var result = await _coinService.GetActiveAIConversationPackagesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("withdraw/update/{transactionId}")]
        public async Task<IActionResult> UpdateWithdraw(Guid transactionId,[FromBody] UpdateWithdrawalDTO dto)
        {
            try
            {
                // Validate DTO
                if (dto == null)
                {
                    return BadRequest(new ResponseDTO
                    {
                        IsSucess = false,
                        BusinessCode = BusinessCode.INVALID_INPUT,
                        Message = "Dữ liệu gửi lên không hợp lệ."
                    });
                }

                // Lấy userId từ token
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    return Unauthorized(new ResponseDTO
                    {
                        IsSucess = false,
                        BusinessCode = BusinessCode.AUTH_NOT_FOUND,
                        Message = "Không xác định được người dùng từ token."
                    });
                }

                // Gọi service
                var result = await _coinService.UpdateWithdrawalAsync(
                    transactionId,
                    userId,
                    dto.NewAmountMoney,
                    dto.BankName,
                    dto.AccountNumber
                );

                return StatusCode(result.IsSucess ? 200 : 400, result);
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

        [HttpGet("transactions/all")]
        public async Task<IActionResult> GetAllTransactions(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? status = null,      // Pending, Completed, Rejected...     
    [FromQuery] string? type = null,
    [FromQuery] string? search = null)
        {
            try
            {
                // Gọi service đã được fix ở tin trước
                var result = await _coinService.GetAllTransactionsAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    status: status,
                    type: type,
                    search: search);

                return StatusCode(result.IsSucess ? StatusCodes.Status200OK
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
        [HttpGet("export/transactions")]
        public async Task<IActionResult> ExportTransactions()
        {
            var pdf = await _coinService.ExportTransactionPdfAsync();
            return File(pdf, "application/pdf", "transaction-report.pdf");
        }
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetTransactionDashboard()
        {
            var result = await _coinService.GetTransactionDashboardAsync();

            if (!result.IsSucess)
                return BadRequest(result);

            return StatusCode(result.IsSucess ? 200 : 400, result);
        }


    }
}
