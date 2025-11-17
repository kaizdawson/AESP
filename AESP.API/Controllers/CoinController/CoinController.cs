using AESP.Common.DTOs;
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

    }
}
