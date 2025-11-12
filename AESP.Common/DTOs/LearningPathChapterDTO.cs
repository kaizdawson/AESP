using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class UpdateLearningPathChapterProgressDTO
    {
        public Guid LearnerCourseId { get; set; }
        public double Progress { get; set; }
    }


    public class CreateLearningPathChapterRequestDTO
    {
        public Guid LearnerCourseId { get; set; }
    }
}
