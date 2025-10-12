using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    // ✅ CREATE DTO
    public class CreateChapterDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
        public Guid CourseId { get; set; }
        public List<CreateChapterExerciseDTO>? Exercises { get; set; }
    }

    public class CreateChapterExerciseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }
    }

    // ✅ UPDATE DTO
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
    }

    // ✅ READ DTO
    public class ReadChapterDTO
    {
        public Guid ChapterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CourseId { get; set; }
        public ReadCourseDTO? Course { get; set; }
        public List<ReadChapterExerciseDTO>? Exercises { get; set; }
    }

    public class ReadChapterExerciseDTO
    {
        public Guid ExerciseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int NumberOfQuestion { get; set; }
        public Guid ChapterId { get; set; }
    }
}
