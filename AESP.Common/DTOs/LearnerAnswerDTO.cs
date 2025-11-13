using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class SubmitLearnerAnswerDTO
    {

        [Required]
        public string AudioRecordingUrl { get; set; } = string.Empty;

        public string TranscribedText { get; set; } = string.Empty;

        [Range(0, 100)]
        public int ScoreForVoice { get; set; }

        public string ExplainTheWrongForVoiceAI { get; set; } = string.Empty;

        //// Dùng trong tương lai
        //public string PronunciationJson { get; set; } = string.Empty;  // điểm IPA chi tiết
        //public bool IsSkipped { get; set; } = false;                  // học viên skip
        //public int DurationInMs { get; set; } = 0;                    // thời gian record
    }
}
