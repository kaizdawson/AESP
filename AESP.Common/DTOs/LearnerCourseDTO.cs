using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{

    public enum LearnerCourseStatus
    {
        Enrolled = 1,   // Đang học
        Completed = 2,  // Hoàn thành
        Cancelled = 3   // Đã hủy
    }

    // 🔹 CREATE
    public class CreateLearnerCourseDTO
    {
        public Guid LearnerProfileId { get; set; }
        public int NumberOfCourse { get; set; }
        public Guid CourseId { get; set; }
    }

    // 🔹 READ
    public class ReadLearnerCourseDTO
    {
        public Guid LearnerCourseId { get; set; }
        public Guid LearnerProfileId { get; set; }
        public DateTime GeneratedDate { get; set; }
        public LearnerCourseStatus Status { get; set; }
        public int NumberOfCourse { get; set; }
        public double Progress { get; set; }

        // Thông tin phụ
        public string? CourseTitle { get; set; }
        public string? Level { get; set; }
    }

    // 🔹 UPDATE
    public class UpdateProgressLearnerCourseDTO
    {
        public Guid LearnerCourseId { get; set; }
        public double Progress { get; set; }
    }
}
