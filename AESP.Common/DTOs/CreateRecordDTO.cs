using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class CreateRecordDTO
    {
        public string AudioRecordingURL { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
