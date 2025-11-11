using System;

namespace AESP.Common.DTOs
{
    public class CreateLearningPathCourseDTO
    {
        public Guid LearnerCourseId { get; set; } // ✅ thay LearningPathId bằng LearnerCourseId
        public Guid CourseId { get; set; }
    }

    public class ReadLearningPathCourseDTO
    {
        public Guid LearningPathCourseId { get; set; }
        public Guid LearnerCourseId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Progress { get; set; }
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
    }

    public class UpdateLearningPathCourseDTO
    {
        public string? Status { get; set; }
        public double? Progress { get; set; }
    }
}
