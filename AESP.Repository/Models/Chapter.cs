using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Chapter
    {
        [Key]
        public Guid ChapterId { get; set; }
        public Guid TopicId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Level { get; set; }
        public DateTime CreatedAt { get; set; }
        public int NumberOfModule { get; set; }




        public Topic Topic { get; set; } = null!;
        public ICollection<ChapterModule> ChapterModules { get; set; } = new List<ChapterModule>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<LearningPathChapter> LearningPathChapters { get; set; } = new List<LearningPathChapter>();

    }
}
