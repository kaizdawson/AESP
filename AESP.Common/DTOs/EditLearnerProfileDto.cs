using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class EditLearnerProfileDto
    {
        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [MinLength(3, ErrorMessage = "Họ và tên phải có ít nhất 3 ký tự.")]
        [MaxLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
        [RegularExpression(
        @"^[A-Za-zÀ-ỹ\s]+$",
        ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng."
    )]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải có 10 chữ số và bắt đầu bằng 0.")]
        public string PhoneNumber { get; set; } = null!;
    }
}
