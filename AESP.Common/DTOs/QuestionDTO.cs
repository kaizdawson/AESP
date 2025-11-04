using System;
using System.Collections.Generic;

namespace AESP.Common.DTOs
{
    // ---------------- ENUM ----------------
    public enum QuestionType
    {
        Word,
        Phrase,
        Sentence
    }

    // ---------------- CREATE ----------------
    public class CreateQuestionDTO
    {
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public int OrderIndex { get; set; }
        public string PhonemeJson { get; set; } = string.Empty;

        public List<CreateQuestionMediaDTO>? Media { get; set; }
    }

    // ---------------- UPDATE ----------------
    public class UpdateQuestionDTO
    {
        public string? Text { get; set; }
        public QuestionType? Type { get; set; }
        public int? OrderIndex { get; set; }
        public string? PhonemeJson { get; set; }

        public List<UpdateQuestionMediaDTO>? Media { get; set; }
    }

    // ---------------- READ ----------------
    public class ReadQuestionDTO
    {
        public Guid QuestionId { get; set; }
        public Guid ExerciseId { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string PhonemeJson { get; set; } = string.Empty;

        public List<ReadQuestionMediaDTO>? Media { get; set; }
    }

    // ---------------- MEDIA DTOs ----------------
    public class CreateQuestionMediaDTO
    {
        public string Accent { get; set; } = string.Empty;
        public string? AudioURL { get; set; }
        public string? VideoURL { get; set; }
        public string? ImageURL { get; set; }
        public string? Source { get; set; }
    }

    public class UpdateQuestionMediaDTO : CreateQuestionMediaDTO
    {
        public Guid QuestionMediaId { get; set; }
    }

    public class ReadQuestionMediaDTO
    {
        public Guid QuestionMediaId { get; set; }
        public string Accent { get; set; } = string.Empty;
        public string? AudioURL { get; set; }
        public string? VideoURL { get; set; }
        public string? ImageURL { get; set; }
        public string? Source { get; set; }
    }
}
