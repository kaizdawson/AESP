using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class PracticeDetail
    {
        [Key]
        public Guid PracticeDetailId { get; set; }
        public Guid PracticeSessionId { get; set; }
        public string Sentence { get; set; }
        public string Transcript { get; set; }
        public double PronunciationScore { get; set; }
        public string GrammarCorrection { get; set; }

        // Navigation
        public PracticeSession PracticeSession { get; set; }
    }
}
