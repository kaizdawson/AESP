using AESP.Common.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AESP.Service.Implementation
{
    public class PayOSService
    {
        private readonly HttpClient _httpClient;
        private readonly PayOSConfig _config;
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(HttpClient httpClient, IOptions<PayOSConfig> config, ILogger<PayOSService> logger)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _logger = logger;
        }

        /// <summary>
        /// Tạo link thanh toán PayOS và trả về checkout URL.
        /// </summary>
        public async Task<string> CreatePaymentAsync(
    decimal amount, string userId, int numberOfCoin, string? description = null, int? orderCode = null)
        {
            var returnUrl = "https://aespwithai.com/coin";
            var cancelUrl = "https://aespwithai.com/coin?cancel=true";

            var finalOrderCode = orderCode ?? new Random().Next(100000, 999999);
            var amountMinor = (int)amount;

            var desc = string.IsNullOrEmpty(description) ? "AESP Payment" : description;

            var rawData = $"amount={amountMinor}&cancelUrl={cancelUrl}&description={desc}&orderCode={finalOrderCode}&returnUrl={returnUrl}";
            var signature = GenerateSignature(rawData);

            var payload = new
            {
                orderCode = finalOrderCode,
                amount = amountMinor,
                description = desc,
                cancelUrl,
                returnUrl,
                signature
            };

            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api-merchant.payos.vn/v2/payment-requests");

            request.Headers.Add("x-client-id", _config.ClientId);
            request.Headers.Add("x-api-key", _config.ApiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("📤 Gửi yêu cầu tạo thanh toán PayOS: {Json}", json);

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📥 Phản hồi từ PayOS: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"❌ PayOS Error: {response.StatusCode} – {responseContent}");
            }

            var jsonDoc = JsonDocument.Parse(responseContent);
            if (jsonDoc.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.ValueKind == JsonValueKind.Object &&
                dataElement.TryGetProperty("checkoutUrl", out var checkoutUrlElement))
            {
                return checkoutUrlElement.GetString() ?? throw new Exception("checkoutUrl không hợp lệ trong phản hồi.");
            }

            throw new Exception($"Không tìm thấy checkoutUrl trong phản hồi PayOS: {responseContent}");
        }



        /// <summary>
        /// Sinh chữ ký HMAC SHA256 (dùng cho tạo request thanh toán).
        /// </summary>
        private string GenerateSignature(string rawData)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.ChecksumKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Xác thực chữ ký webhook từ PayOS (nếu cần verify thủ công).
        /// </summary>
        public bool VerifyWebhookSignature(Dictionary<string, object> payload, string providedSignature)
        {
            var flatData = new Dictionary<string, string>();
            foreach (var kvp in payload)
            {
                flatData[kvp.Key] = kvp.Value?.ToString() ?? "";
            }

            var sorted = flatData.OrderBy(k => k.Key, StringComparer.Ordinal);
            var rawData = string.Join("&", sorted.Select(kvp => $"{kvp.Key}={kvp.Value}"));

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.ChecksumKey));
            var computedHash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData)))
                .Replace("-", "").ToLower();

            return string.Equals(computedHash, providedSignature, StringComparison.OrdinalIgnoreCase);
        }
    }
}
