using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ReadReviewHistoryDTO
    {
        public Guid ReviewId { get; set; }
        public Guid? LearnerAnswerId { get; set; }
        public Guid? RecordId { get; set; }
        public double Score { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Thông tin thêm để hiển thị
        public string? QuestionContent { get; set; }
        public string? LearnerFullName { get; set; }
    }
}
