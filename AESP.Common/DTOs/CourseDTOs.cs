using System;
using System.Collections.Generic;

namespace AESP.Common.DTOs
{
    public enum CourseLevel
    {
        A1, A2, B1, B2, C1, C2
    }

    // ----------------------------
    // 🔹 CREATE DTO
    // ----------------------------
    public class CreateCourseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public CourseLevel Level { get; set; }

        // ✅ Danh sách chapter đi kèm khi tạo course
        public List<CreateCourseChapterDTO>? Chapters { get; set; }
    }

    public class CreateCourseChapterDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
    }

    // ----------------------------
    // 🔹 UPDATE DTO
    // ----------------------------
    public class UpdateCourseDTO
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public int? NumberOfChapter { get; set; }
        public int? OrderIndex { get; set; }
        public CourseLevel? Level { get; set; }

        public List<UpdateCourseChapterDTO>? Chapters { get; set; }
    }

    public class UpdateCourseChapterDTO
    {
        public Guid? ChapterId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? NumberOfExercise { get; set; }
    }

    // ----------------------------
    // 🔹 READ DTO
    // ----------------------------
    public class ReadCourseDTO
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }

        // ✅ Khi trả về đọc enum -> string
        public string Level { get; set; } = string.Empty;

        // ✅ Load luôn Chapter & LearningPathCourses
        public List<ReadCourseChapterDTO>? Chapters { get; set; }
        public List<ReadLearningPathCourseDTO>? LearningPathCourses { get; set; }
    }

    // ⚙️ Đây là class đổi tên để tránh trùng với ChapterDTO
    public class ReadCourseChapterDTO
    {
        public Guid ChapterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
    }

    public class ReadLearningPathCourseDTO
    {
        public Guid LearningPathCourseId { get; set; }
        public Guid CourseId { get; set; }
        public Guid LearningPathId { get; set; }
    }
}