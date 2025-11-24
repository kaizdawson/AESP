using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class SubmitRecordUpdateDTO
    {
        public string AudioRecordingURL { get; set; }
        public int Score { get; set; }
        public string AIFeedback { get; set; }
    }

}
