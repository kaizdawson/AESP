using System;
using System.Collections.Generic;

namespace AESP.Common.DTOs
{
    public class CreateExerciseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfQuestion { get; set; }
    }


    public class CreateExerciseQuestionDTO
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string PhonemeJson { get; set; } = string.Empty;
    }

    // ============================================================
    // 🔹 UPDATE
    // ============================================================
    public class UpdateExerciseDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? OrderIndex { get; set; }
        public int? NumberOfQuestion { get; set; }
    }

    public class UpdateExerciseQuestionDTO
    {
        public Guid QuestionId { get; set; }
        public string? Text { get; set; }
        public string? Type { get; set; }
        public int? OrderIndex { get; set; }
        public string? PhonemeJson { get; set; }
    }

    // ============================================================
    // 🔹 READ
    // ============================================================
    public class ReadExerciseDTO
    {
        public Guid ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }
        public Guid ChapterId { get; set; }

        public List<ReadExerciseQuestionDTO>? Questions { get; set; }
    }

    public class ReadExerciseQuestionDTO
    {
        public Guid QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string PhonemeJson { get; set; } = string.Empty;
    }
}
