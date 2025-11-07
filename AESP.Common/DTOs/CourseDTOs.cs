using System;
using System.Collections.Generic;

namespace AESP.Common.DTOs
{
    public enum CourseLevel
    {
        A1, A2, B1, B2, C1, C2
    }

    // ============================================================
    // 🔹 FULL STRUCTURE: COURSE → CHAPTER → EXERCISE → QUESTION
    // ============================================================

    // 🔹 QUESTION
    public class CreateCourseQuestionForCourseDTO
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string PhonemeJson { get; set; } = string.Empty;
    }

    public class ReadCourseQuestionForCourseDTO
    {
        public Guid QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string PhonemeJson { get; set; } = string.Empty;
    }

    public class UpdateCourseQuestionForCourseDTO
    {
        public Guid QuestionId { get; set; }
        public string? Text { get; set; }
        public string? Type { get; set; }
        public int? OrderIndex { get; set; }
        public string? PhonemeJson { get; set; }
    }

    // 🔹 EXERCISE
    public class CreateCourseExerciseForCourseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }
        public List<CreateCourseQuestionForCourseDTO>? Questions { get; set; }
    }

    public class ReadCourseExerciseForCourseDTO
    {
        public Guid ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }


        public bool IsFree { get; set; }

        public List<ReadCourseQuestionForCourseDTO>? Questions { get; set; }
    }

    public class UpdateCourseExerciseForCourseDTO
    {
        public Guid ExerciseId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? OrderIndex { get; set; }
        public int? NumberOfQuestion { get; set; }
        public List<UpdateCourseQuestionForCourseDTO>? Questions { get; set; }
    }

    // 🔹 CHAPTER
    public class CreateCourseChapterForCourseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
        public List<CreateCourseExerciseForCourseDTO>? Exercises { get; set; }
    }

    public class ReadCourseChapterForCourseDTO
    {
        public Guid ChapterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int NumberOfExercise { get; set; }
        public List<ReadCourseExerciseForCourseDTO>? Exercises { get; set; }
    }

    public class UpdateCourseChapterForCourseDTO
    {
        public Guid ChapterId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? NumberOfExercise { get; set; }
        public List<UpdateCourseExerciseForCourseDTO>? Exercises { get; set; }
    }

    // 🔹 COURSE
    public class CreateCourseFullDTO
    {
        public string Title { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public CourseLevel Level { get; set; }
        public decimal Price { get; set; }
        public List<CreateCourseChapterForCourseDTO>? Chapters { get; set; }
    }

    public class ReadCourseFullDTO
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public string Level { get; set; } = string.Empty;

        // ✅ Giá mỗi khóa học (set trong DB)
        public decimal Price { get; set; }

        // ✅ Tính toán runtime: khóa này free hay không
        public bool IsFree { get; set; }

        public List<ReadCourseChapterForCourseDTO>? Chapters { get; set; }
    }

    public class UpdateCourseFullDTO
    {
        public string? Title { get; set; }
        public int? NumberOfChapter { get; set; }
        public int? OrderIndex { get; set; }
        public CourseLevel? Level { get; set; }
        public List<UpdateCourseChapterForCourseDTO>? Chapters { get; set; }
    }


    // ✅ Dùng riêng cho Swagger input (Create/Update Course)
    public class CreateSimpleCourseDTO
    {
        public string Title { get; set; } = string.Empty;
        public int NumberOfChapter { get; set; }
        public int OrderIndex { get; set; }
        public CourseLevel Level { get; set; }
        public decimal Price { get; set; }

        // chỉ chứa mảng Chapter — không có Exercise/Question
        //public List<CreateSimpleCourseChapterDTO>? Chapters { get; set; }
    }

    public class CreateSimpleCourseChapterDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
    }

    // ✅ Update cũng tương tự
    public class UpdateSimpleCourseDTO
    {
        public string? Title { get; set; }
        public int? NumberOfChapter { get; set; }
        public int? OrderIndex { get; set; }
        public CourseLevel? Level { get; set; }
        public decimal? Price { get; set; }  // ✅ thêm để chỉnh giá
    }

    public class UpdateSimpleCourseChapterDTO
    {
        public Guid ChapterId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? NumberOfExercise { get; set; }
    }

}
