using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class StressResult
    {
        [Key]
        public Guid StressResultId { get; set; }
        public Guid PhonemeResultId { get; set; }

        [ForeignKey("PhonemeResultId")]
        public virtual PhonemeResult PhonemeResult { get; set; }
    }
}
