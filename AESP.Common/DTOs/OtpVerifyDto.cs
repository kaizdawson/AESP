using System.ComponentModel.DataAnnotations;

namespace AESP.Common.DTOs
{
    public class OtpVerifyDto
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "OTP không được để trống.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP phải có đúng 6 ký tự.")]
        public string Otp { get; set; } = null!;
    }
}
