using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ResetPasswordByLinkDto
    {
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$", ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự, có ít nhất 1 chữ hoa, 1 số và 1 ký tự đặc biệt.")]
        [DefaultValue("Abc@123")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu xác nhận không được để trống.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
