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
                var checkoutUrl = await _coinService.AddCoinAsync(request.ServicePackageId, userId);
                return Ok(new { checkoutUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelTransaction([FromBody] string orderCode)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("Access token không hợp lệ hoặc thiếu UserId.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("UserId trong token không hợp lệ.");

            try
            {
                await _coinService.CancelTransactionAsync(userId, orderCode);
                return Ok(new { message = "Giao dịch đã hủy thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



    }
}
