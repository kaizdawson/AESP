using System.ComponentModel.DataAnnotations;

namespace AESP.Common.DTOs
{
    public class SendOtpDto
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;
    }
}
