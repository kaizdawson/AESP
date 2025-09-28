using System.ComponentModel.DataAnnotations;

namespace AESP.Common.DTOs
{
    public class SendOtpDto
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email phải đúng định dạng example@gmail.com.")]
        public string Email { get; set; } = string.Empty;
    }
}
