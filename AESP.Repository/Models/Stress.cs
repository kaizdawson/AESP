using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Stress
    {
        [Key]
        public Guid StressId { get; set; }

        [ForeignKey("PhonemeTemplate")]
        public Guid PhonemeId { get; set; }

        [Required]
        public string StressType { get; set; }
        public int SyllableIndex { get; set; }

        public virtual PhonemeTemplate PhonemeTemplate { get; set; }
    }
}
