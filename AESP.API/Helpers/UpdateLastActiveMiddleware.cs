using AESP.Repository.Contract;
using AESP.Repository.Models;
using System.Security.Claims;

namespace AESP.API.Helpers
{
    public class UpdateLastActiveMiddleware
    {
        private readonly RequestDelegate _next;

        public UpdateLastActiveMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IGenericRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            try
            {
                // ✅ BỎ QUA webhook PayOS (không yêu cầu token)
                if (context.Request.Path.StartsWithSegments("/api/coin/payos-webhook", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }

                // ✅ Chỉ xử lý nếu user có token hợp lệ (đã đăng nhập)
                if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
                {
                    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
                    {
                        var user = await userRepository.GetById(userId);

                        if (user != null)
                        {
                            user.LastActiveAt = DateTime.UtcNow;

                            await userRepository.Update(user);
                            await unitOfWork.SaveChangeAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ UpdateLastActiveMiddleware error: " + ex.Message);
            }

            await _next(context);
        }
    }
}
