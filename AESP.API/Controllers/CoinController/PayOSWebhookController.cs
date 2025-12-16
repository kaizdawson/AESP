using AESP.API.Helpers;
using AESP.Service.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AESP.API.Controllers.CoinController
{
    [AllowAnonymous]
    [Route("api/coin/payos-webhook")]
    [ApiController]
    public class PayOSWebhookController : ControllerBase
    {
        private readonly ILogger<PayOSWebhookController> _logger;
        private readonly ICoinService _coinService;
        private readonly IConfiguration _config;

        public PayOSWebhookController(
            ILogger<PayOSWebhookController> logger,
            ICoinService coinService,
            IConfiguration config)
        {
            _logger = logger;
            _coinService = coinService;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] JsonElement payload)
        {
            try
            {
                _logger.LogInformation("📩 Webhook PayOS nhận vào lúc {Time}", DateTimeHelper.NowVN());

                var checksumKey = _config["PayOS:ChecksumKey"];
                if (string.IsNullOrEmpty(checksumKey))
                    return BadRequest(new { error = "Missing ChecksumKey in config." });

               
                if (!payload.TryGetProperty("data", out var dataElement) ||
                    !payload.TryGetProperty("signature", out var sigElement))
                {
                    _logger.LogWarning("⚠️ Webhook không có field data hoặc signature");
                    return BadRequest(new { error = "Invalid payload" });
                }

      
                var dataDict = new SortedDictionary<string, string>(StringComparer.Ordinal);
                foreach (var prop in dataElement.EnumerateObject())
                {
                    dataDict[prop.Name] = prop.Value.ToString();
                }

                var dataRaw = string.Join("&", dataDict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                string providedSignature = sigElement.GetString() ?? "";

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
                var computedHash = BitConverter.ToString(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(dataRaw))
                ).Replace("-", "").ToLower();

                _logger.LogInformation("🔑 provided={Provided} | computed={Computed}", providedSignature, computedHash);
                _logger.LogInformation("🧩 dataRaw={Data}", dataRaw);

                if (!string.Equals(providedSignature, computedHash, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("❌ Sai chữ ký webhook!");
                    return Unauthorized(new { error = "Invalid signature" });
                }

                decimal amount = dataElement.GetProperty("amount").GetDecimal();
                string orderCode = dataElement.GetProperty("orderCode").ToString();

                _logger.LogInformation("💰 Webhook hợp lệ cho orderCode={OrderCode}, amount={Amount}", orderCode, amount);

                await _coinService.AddBalanceFromPayOSAsync(orderCode, amount, orderCode);

                return Ok(new { message = "✅ Coin balance updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi xử lý webhook PayOS");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Ping()
        {
            return Ok(new { message = "PayOS Webhook đang hoạt động 🚀" });
        }
    }
}
