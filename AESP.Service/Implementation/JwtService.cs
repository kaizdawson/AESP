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

        public string GenerateAccessToken(User user)
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
                expires: DateTime.UtcNow.AddMinutes(60), // 1 tiếng chắc đủ rồi
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


        // 🧩 Giải mã token reset password, lấy email ra
        public string? ValidateAndGetEmailFromResetToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"]!);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _config["JWT:ValidIssuer"],
                    ValidAudience = _config["JWT:ValidAudience"],
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                // chỉ dùng token đúng loại reset-password
                var typeClaim = principal.FindFirst("type")?.Value;
                if (typeClaim != "reset-password") return null;

                return principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            }
            catch
            {
                return null;
            }
        }

    }

}
