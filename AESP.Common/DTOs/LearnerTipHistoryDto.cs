using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class LearnerTipHistoryDto
    {
        public Guid TipTransactionId { get; set; }
        public Guid ReviewId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int TipAmount { get; set; }
        public string TipMessage { get; set; } = string.Empty; // Nội dung reviewer gửi kèm tip
        public DateTime TipCreatedAt { get; set; }
        public string ReviewType { get; set; } = string.Empty; // LearnerAnswer hoặc Record
        public double? ReviewScore { get; set; }
        public string? ReviewComment { get; set; }
        public string? LearnerAudioUrl { get; set; }         // Audio của learner (đã có)
        public string? ReviewerAudioUrl { get; set; }        // ← THÊM: Audio feedback từ reviewer
    }

}
