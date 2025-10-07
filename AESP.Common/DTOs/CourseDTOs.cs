using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class CreateCourseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public string Level { get; set; } = string.Empty;
    }

    public class UpdateCourseDTO
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public int? NumberOfChapter { get; set; }
        public int? OrderIndex { get; set; }
        public string? Level { get; set; }
    }

    public class ReadCourseDTO
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public string Level { get; set; } = string.Empty;
    }
}
