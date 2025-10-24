using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AESP.Repository.DB;
using AESP.Repository.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AESP.Service.Implementation
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public JwtService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public string GenerateAccessToken(User user, bool? isPlacementTestDone = null, bool isReviewerActive = false)
        {
            // Lấy role từ DB nghen
            var role = string.IsNullOrEmpty(user.Role) ? "User" : user.Role;

            // Tạo claims nè
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim("FullName", user.FullName),
                new Claim("PhoneNumber", user.PhoneNumber),
                new Claim(ClaimTypes.Role, role),
                new Claim("IsPlacementTestDone", (isPlacementTestDone ?? false).ToString()),
                new Claim("IsReviewerActive", isReviewerActive.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Secret key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            // Tạo token ở đây
            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // 15 p chắc đủ rồi
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 🧩 Sinh JWT cho link reset password (chỉ có email và type)
        public string GenerateResetPasswordToken(string email)
        {
            var claims = new List<Claim>
        {
        new Claim(JwtRegisteredClaimNames.Sub, email),
        new Claim("type", "reset-password"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15), // chỉ sống 15 phút
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public string? ValidateAndGetEmailFromResetToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("❌ Token trống hoặc null");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"]!);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _config["JWT:ValidIssuer"],
                ValidateAudience = true,
                ValidAudience = _config["JWT:ValidAudience"],
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            try
            {
                Console.WriteLine($"🔍 BẮT ĐẦU KIỂM TRA TOKEN...");
                Console.WriteLine($"  ➤ Issuer trong config: {_config["JWT:ValidIssuer"]}");
                Console.WriteLine($"  ➤ Audience trong config: {_config["JWT:ValidAudience"]}");
                Console.WriteLine($"  ➤ Secret length: {_config["JWT:Secret"]?.Length}");

                var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("❌ Thuật toán không khớp (không phải HS512).");
                    return null;
                }

                var type = principal.FindFirst("type")?.Value;
                if (type != "reset-password")
                {
                    Console.WriteLine($"❌ Claim 'type' không hợp lệ: {type}");
                    return null;
                }

                var email =
    principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
    principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
    principal.FindFirst("sub")?.Value ??
    principal.Identity?.Name;

                Console.WriteLine($"📩 Email claim đọc được: {email ?? "null"}");
                return string.IsNullOrEmpty(email) ? null : email;

            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ JWT validation failed!");
                Console.WriteLine($"  ➤ Lỗi chính: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"  ➤ Inner: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                return null;
            }


        }




    }

}
