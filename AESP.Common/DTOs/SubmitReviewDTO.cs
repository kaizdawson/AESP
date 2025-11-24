using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class SubmitReviewDTO
    {
        public Guid? LearnerAnswerId { get; set; }

        public Guid? RecordId { get; set; }

        [Required(ErrorMessage = "ReviewerProfileId không được để trống.")]
        public Guid ReviewerProfileId { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm đánh giá phải nằm trong khoảng 0 - 10.")]
        public double Score { get; set; }

        [Required(ErrorMessage = "Nhận xét không được để trống.")]
        public string Comment { get; set; } = string.Empty;

        [Url(ErrorMessage = "RecordAudioUrl phải là URL hợp lệ.")]
        public string? RecordAudioUrl { get; set; }
    }
}
