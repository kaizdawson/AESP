using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Booking
    {
        [Key]
        public Guid BookingId { get; set; }
        public string Status { get; set; } = string.Empty;


        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; } = null!;

        public Guid MentorScheduleId { get; set; }
        [ForeignKey(nameof(MentorScheduleId))]
        public MentorSchedule MentorSchedule { get; set; } = null!;

        public Guid PracticeSessionId { get; set; }
        [ForeignKey(nameof(PracticeSessionId))]
        public PracticeSession PracticeSession { get; set; } = null!;

    }
}
