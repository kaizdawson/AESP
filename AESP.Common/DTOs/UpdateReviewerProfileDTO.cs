using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class UpdateReviewerProfileDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập kinh nghiệm giảng dạy.")]
        [StringLength(500, ErrorMessage = "Kinh nghiệm không được vượt quá 500 ký tự.")]
        public string Experience { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn trình độ.")]
        [RegularExpression(@"^(A1|A2|B1|B2|C1|C2)$", ErrorMessage = "Trình độ không hợp lệ (A1 - C2).")]
        public string Levels { get; set; } = string.Empty;
    }
}
