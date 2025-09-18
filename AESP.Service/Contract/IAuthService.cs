using AESP.Common.DTOs;

namespace AESP.Service.Contract
{
    public interface IAuthService
    {
        Task<LoginResult> SignUpAsync(SignUpDto dto, int roleId);
        Task<LoginResult> SignInAsync(LoginRequest request);

        Task<LoginResult> RefreshTokenAsync(RefreshTokenRequestDto dto);

    }
}
