using AESP.Common.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit.Encodings;
using SkiaSharp;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QRCoder;
using SkiaSharp;

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
        public async Task<(string checkoutUrl, string orderCode, string qrCode, string qrDataURL)> CreatePaymentAsync(
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

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api-merchant.payos.vn/v2/payment-requests");
            request.Headers.Add("x-client-id", _config.ClientId);
            request.Headers.Add("x-api-key", _config.ApiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"PayOS Error: {response.StatusCode} – {responseContent}");

            var jsonDoc = JsonDocument.Parse(responseContent);
            var data = jsonDoc.RootElement.GetProperty("data");

            var checkoutUrl = data.GetProperty("checkoutUrl").GetString() ?? "";
            var orderCodeStr = data.GetProperty("orderCode").GetRawText();

            // ⚠️ lấy cả qrCode và qrDataURL nếu có
            var qrCode = data.TryGetProperty("qrCode", out var qrCodeProp)
                ? qrCodeProp.GetString() ?? ""
                : "";

            string qrDataURL = "";
            if (data.TryGetProperty("qrDataURL", out var qrDataUrlProp))
                qrDataURL = qrDataUrlProp.GetString() ?? "";
            else if (!string.IsNullOrEmpty(qrCode))
                qrDataURL = GenerateQrBase64(qrCode);

            return (checkoutUrl, orderCodeStr, qrCode, qrBase64: qrDataURL);
        }


        private string GenerateQrBase64(string qrContent)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.Q);
            var matrix = qrCodeData.ModuleMatrix;

            int pixelsPerModule = 20;
            int size = matrix.Count * pixelsPerModule;

            using var surface = SKSurface.Create(new SKImageInfo(size, size));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            using var paint = new SKPaint { Color = SKColors.Black };
            for (int y = 0; y < matrix.Count; y++)
            {
                for (int x = 0; x < matrix.Count; x++)
                {
                    if (matrix[y][x])
                        canvas.DrawRect(x * pixelsPerModule, y * pixelsPerModule, pixelsPerModule, pixelsPerModule, paint);
                }
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return "data:image/png;base64," + Convert.ToBase64String(data.ToArray(), Base64FormattingOptions.None);

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
