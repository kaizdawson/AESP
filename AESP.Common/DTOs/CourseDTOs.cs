using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public enum CourseLevel
    {
        A1,
        A2,
        B1,
        B2,
        C1,
        C2
    }

    public class CreateCourseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }

        // ✅ Đổi Level sang Enum
        public CourseLevel Level { get; set; }
    }

    public class UpdateCourseDTO
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public int? NumberOfChapter { get; set; }
        public int? OrderIndex { get; set; }

        // ✅ Nullable enum cho Update
        public CourseLevel? Level { get; set; }
    }

    public class ReadCourseDTO
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }

        // ✅ Khi trả về đọc enum -> string
        public string Level { get; set; } = string.Empty;
    }
}