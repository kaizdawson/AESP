using AESP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class ReadLearnerCourseDTOS
    {
       
       
        public CourseStatus Status { get; set; }   // <-- enum cho FE
        public double Progress { get; set; }

        // Thông tin Course map theo NumberOfCourse -> Course.OrderIndex
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Duration { get; set; } // ngày
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    public class ReadLearnerDetailDTO
    {
        public Guid LearnerProfileId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public double PronunciationScore { get; set; }
        public int DailyMinutes { get; set; }
        public string Status { get; set; } = string.Empty; // trạng thái user (Active/Banned)
        public DateTime JoinDate { get; set; }
        public DateTime? LastActiveAt { get; set; }

        // Khóa học hiện tại (nếu có)
        public List<ReadLearnerCourseDTOS> Courses { get; set; } = new();

        // Thống kê
        public int AssessmentCount { get; set; }
        public double AvgScore { get; set; }
    }
}
