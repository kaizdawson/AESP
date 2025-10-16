using System;
using System.Collections.Generic;

namespace AESP.Common.DTOs
{
    // ============================================================
    // 🔹 CREATE
    // ============================================================
    public class CreateChapterDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
        public Guid CourseId { get; set; }   // để biết chương thuộc khóa học nào

        public List<CreateChapterExerciseDTO>? Exercises { get; set; }
    }

    public class CreateChapterExerciseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }

        // ✅ mỗi exercise có thể có nhiều question
        public List<CreateChapterQuestionDTO>? Questions { get; set; }
    }

    public class CreateChapterQuestionDTO
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string IPA { get; set; } = string.Empty;
        public string PhonemeJson { get; set; } = string.Empty;
    }

    // ============================================================
    // 🔹 UPDATE
    // ============================================================
    public class UpdateChapterDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? NumberOfExercise { get; set; }
        public Guid CourseId { get; set; }

        public List<UpdateChapterExerciseDTO>? Exercises { get; set; }
    }

    public class UpdateChapterExerciseDTO
    {
        public Guid ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? OrderIndex { get; set; }
        public int? NumberOfQuestion { get; set; }

        public List<UpdateChapterQuestionDTO>? Questions { get; set; }
    }

    public class UpdateChapterQuestionDTO
    {
        public Guid QuestionId { get; set; }
        public string? Text { get; set; }
        public string? Type { get; set; }
        public int? OrderIndex { get; set; }
        public string? IPA { get; set; }
        public string? PhonemeJson { get; set; }
    }

    // ============================================================
    // 🔹 READ
    // ============================================================
    public class ReadChapterDTO
    {
        public Guid ChapterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
        public DateTime CreatedAt { get; set; }

        // ✅ danh sách bài tập trong chương
        public List<ReadChapterExerciseDTO>? Exercises { get; set; }
    }

    public class ReadChapterExerciseDTO
    {
        public Guid ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }

        // ✅ lồng danh sách câu hỏi bên trong
        public List<ReadChapterQuestionDTO>? Questions { get; set; }
    }

    public class ReadChapterQuestionDTO
    {
        public Guid QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string IPA { get; set; } = string.Empty;
        public string PhonemeJson { get; set; } = string.Empty;
    }

    // 🔹 CREATE (gọn)
    public class CreateSimpleChapterDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
        public Guid CourseId { get; set; }
        public List<CreateSimpleExerciseDTO>? Exercises { get; set; }
    }

    public class CreateSimpleExerciseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }
    }

    // 🔹 UPDATE (gọn)
    public class UpdateSimpleChapterDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? NumberOfExercise { get; set; }
        public List<UpdateSimpleExerciseDTO>? Exercises { get; set; }
    }

    public class UpdateSimpleExerciseDTO
    {
        public Guid ExerciseId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? OrderIndex { get; set; }
        public int? NumberOfQuestion { get; set; }
    }

}
