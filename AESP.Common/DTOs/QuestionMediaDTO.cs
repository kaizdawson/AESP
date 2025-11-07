using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    // ============================================================
    // 🔹 CREATE (V2)
    // ============================================================
    public class CreateQuestionMediaV2DTO
    {
        public Guid QuestionId { get; set; }  // ✅ Gắn vào QuestionId để tạo nhanh media theo câu hỏi
        public string Accent { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
    }

    // ============================================================
    // 🔹 UPDATE (V2)
    // ============================================================
    public class UpdateQuestionMediaV2DTO
    {
        public string Accent { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
    }

    // ============================================================
    // 🔹 READ (V2)
    // ============================================================
    public class ReadQuestionMediaV2DTO
    {
        public Guid QuestionMediaId { get; set; }
        public Guid QuestionId { get; set; }
        public string Accent { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? Source { get; set; }
    }
}
