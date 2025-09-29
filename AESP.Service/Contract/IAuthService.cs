using AESP.Common.DTOs;

namespace AESP.Service.Contract
{
    public interface IAuthService
    {
        Task<LoginResult> SignUpAsync(SignUpDto dto);
        Task<LoginResult> SignInAsync(LoginRequest request);

        Task<LoginResult> RefreshTokenAsync(RefreshTokenRequestDto dto);


        Task SendOtpAsync(string email);
        Task<(bool Success, string Message)> VerifyOtpAsync(OtpVerifyDto dto);

        Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);

        Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequestDto dto);
        Task<(bool Success, string Message)> ResetPasswordByLinkAsync(ResetPasswordByLinkDto dto);

    }
}
