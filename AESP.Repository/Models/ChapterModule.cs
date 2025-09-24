using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ChapterModule
    {
        [Key]
        public Guid ModuleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int Amount { get; set; }



        public Guid ChapterId { get; set; }
        [ForeignKey(nameof(ChapterId))]
        public Chapter Chapter { get; set; } = null!;
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
