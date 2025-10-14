using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AESP.Common.DTOs
{
    // ----------- Assessment -----------
    public class CreateAssessmentDTO
    {
        public Guid LearnerProfileId { get; set; }               // ✅ ID học viên
        public double Score { get; set; }                        // ✅ Tổng điểm
        public string Type { get; set; } = string.Empty;          // ✅ Loại đánh giá (ví dụ: Speaking, Listening)
        public string Feedback { get; set; } = string.Empty;      // ✅ Nhận xét tổng thể
        public double NumberOfQuestion { get; set; }              // ✅ Tổng số câu hỏi

        // ✅ Danh sách chi tiết (AssessmentDetail)
        public List<CreateAssessmentDetailInAssessmentDTO> AssessmentDetails { get; set; } = new();
    }

    public class UpdateAssessmentDTO
    {
        public double? Score { get; set; }
        public string? Type { get; set; }
        public string? Feedback { get; set; }
        public double? NumberOfQuestion { get; set; }

        public List<UpdateAssessmentDetailInAssessmentDTO>? AssessmentDetails { get; set; }
    }

    public class ReadAssessmentDTO
    {
        public Guid AssessmentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Score { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public double NumberOfQuestion { get; set; }
        public Guid LearnerProfileId { get; set; }

        // ✅ Dữ liệu chi tiết (AssessmentDetail)
        public List<ReadAssessmentDetailInAssessmentDTO> AssessmentDetails { get; set; } = new();
    }

    // ----------- AssessmentDetail (used inside Assessment) -----------
    public class CreateAssessmentDetailInAssessmentDTO
    {
        public double Score { get; set; }                         // ✅ Điểm câu hỏi
        public string Type { get; set; } = string.Empty;           // ✅ Loại câu hỏi
        public string AI_Feedback { get; set; } = string.Empty;    // ✅ Feedback của AI
        public string AnswerAudio { get; set; } = string.Empty;    // ✅ Link audio trả lời
        public Guid QuestionAssessmentId { get; set; }             // ✅ Liên kết tới câu hỏi
        [JsonIgnore] // ✅ Ẩn khỏi Swagger khi tạo Assessment
        public Guid? AssessmentId { get; set; }
    }

    public class UpdateAssessmentDetailInAssessmentDTO
    {
        public Guid AssessmentDetailId { get; set; }
        public double? Score { get; set; }
        public string? Type { get; set; }
        public string? AI_Feedback { get; set; }
        public string? AnswerAudio { get; set; }
    }

    public class ReadAssessmentDetailInAssessmentDTO
    {
        public Guid AssessmentDetailId { get; set; }
        public double Score { get; set; }
        public string Type { get; set; } = string.Empty;
        public string AI_Feedback { get; set; } = string.Empty;
        public string AnswerAudio { get; set; } = string.Empty;
        public Guid QuestionAssessmentId { get; set; }
        [JsonIgnore] // ✅ Ẩn khi trả response ra Swagger
        public Guid AssessmentId { get; set; }
    }

}
