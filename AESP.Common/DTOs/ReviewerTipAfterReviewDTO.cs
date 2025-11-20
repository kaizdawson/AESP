using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ReviewerTipAfterReviewDTO
    {
        [Required]
        public Guid ReviewId { get; set; }           // Bắt buộc phải có ReviewId vừa tạo

        [Range(1, 500, ErrorMessage = "Số coin thưởng phải từ 1 đến 500")]
        public int AmountCoin { get; set; }

        [StringLength(300)]
        public string Message { get; set; } = "Phát âm rất hay, cố lên nhé!";
    }
}
