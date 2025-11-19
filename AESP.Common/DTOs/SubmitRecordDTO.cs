using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class SubmitRecordDTO
    {
        public string AudioRecordingURL { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public double Score { get; set; }
        public string AIFeedback { get; set; } = string.Empty;
    }
}
