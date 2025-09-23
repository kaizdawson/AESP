using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class MentorSchedule
    {
        [Key]
        public Guid MentorScheduleId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;

        public Guid MentorProfileId { get; set; }
        [ForeignKey(nameof(MentorProfileId))]
        public MentorProfile MentorProfile { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}
