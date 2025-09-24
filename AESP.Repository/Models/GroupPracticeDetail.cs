using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class GroupPracticeDetail
    {
        [Key]
        public Guid GroupPracticeDetailId { get; set; }
        public string Content { get; set; } = string.Empty;
        public double PronunciationScore { get; set; }
        public DateTime Timestamp { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public string GrammarCorrection { get; set; } = string.Empty;
        public string VocabularySuggest { get; set; } = string.Empty;


        public Guid GroupSessionId { get; set; }
        [ForeignKey(nameof(GroupSessionId))]
        public GroupSession GroupSession { get; set; } = null!;
    }
}
