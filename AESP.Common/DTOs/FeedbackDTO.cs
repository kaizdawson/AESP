using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class FeedbackDTO
    {
        [Required(ErrorMessage = "Nội dung feedback không được để trống.")]
        [MaxLength(1000, ErrorMessage = "Nội dung không được vượt quá 1000 ký tự.")]
        public string Content { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Rating phải nằm trong khoảng 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Người gửi feedback (UserId) là bắt buộc.")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Reviewer (TargetId) là bắt buộc.")]
        public Guid TargetId { get; set; }
     
    }
}
