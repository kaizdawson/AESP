using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ReviewerProfileUpdateDto
    {
        [Required(ErrorMessage = "Kinh nghiệm không được để trống.")]
        [StringLength(500, ErrorMessage = "Kinh nghiệm tối đa 500 ký tự.")]
        public string Experience { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên không được để trống.")]
        [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải có 10 chữ số và bắt đầu bằng 0.")]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
