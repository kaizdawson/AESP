using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AESP.API.Controllers.CoinController
{
    [Route("api/coin/payos-webhook")]
    [ApiController]
    public class PayOSWebhookController : ControllerBase
    {
        private readonly ILogger<PayOSWebhookController> _logger;

        public PayOSWebhookController(ILogger<PayOSWebhookController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult HandleWebhook([FromBody] object payload)
        {
            _logger.LogInformation("📩 Webhook từ PayOS nhận vào lúc: {Time}", DateTime.UtcNow);
            _logger.LogInformation("Payload: {Payload}", payload?.ToString());
            return Ok(new { message = "Webhook URL active ✅" });
        }

        [HttpGet]
        public IActionResult Ping()
        {
            return Ok(new { message = "PayOS Webhook đang hoạt động 🚀" });
        }
    }
}
