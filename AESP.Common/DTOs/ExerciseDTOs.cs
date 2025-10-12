using System;

namespace AESP.Common.DTOs
{
    public class CreateExerciseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }
        public Guid ChapterId { get; set; }
    }

    public class UpdateExerciseDTO
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int? OrderIndex { get; set; }
        public int? NumberOfQuestion { get; set; }
    }

    public class ReadExerciseDTO
    {
        public Guid ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }
        public Guid ChapterId { get; set; }

        // ✅ Liên kết ngược
        public ReadChapterDTO? Chapter { get; set; }
    }
}
