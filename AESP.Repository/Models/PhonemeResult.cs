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
        public string Type { get; set; } = string.Empty;
        public string PhonemeJson { get; set; } = string.Empty;

        [ForeignKey("LearnerAnswerId")]
        public LearnerAnswer LearnerAnswer { get; set; }

    }
}