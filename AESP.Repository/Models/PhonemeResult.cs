using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class PhonemeResult
    {
        [Key]
        public Guid PhonemeResultId { get; set; }
        public Guid LearnerAnswerId { get; set; }
        public Guid PhonemeId { get; set; }

        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string OrderIndex { get; set; } = string.Empty;
        public string ExpectedSymbol { get; set; } = string.Empty;



        [ForeignKey("LearnerAnswerId")]
        public  LearnerAnswer LearnerAnswer { get; set; }

        [ForeignKey("PhonemeId")]
        public PhonemeTemplate PhonemeTemplate { get; set; }
        public virtual ICollection<StressResult> StressResults { get; set; }
    }
}
