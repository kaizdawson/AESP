using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class CreateChapterDTO
    {
        [Required(ErrorMessage = "Tên chương không được để trống.")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "CourseId không được để trống.")]
        public Guid CourseId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số bài tập phải lớn hơn hoặc bằng 0.")]
        public int NumberOfExercise { get; set; }
    }

    public class UpdateChapterDTO
    {
        [Required(ErrorMessage = "Tên chương không được để trống.")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Số bài tập phải lớn hơn hoặc bằng 0.")]
        public int? NumberOfExercise { get; set; }
    }

    public class ReadChapterDTO
    {
        public Guid ChapterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int NumberOfExercise { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CourseId { get; set; }
    }
}
