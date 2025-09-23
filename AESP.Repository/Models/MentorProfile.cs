using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class MentorProfile
    {
        [Key]
        public Guid MentorProfileId { get; set; }
        public Guid UserId { get; set; }
        public string Skills { get; set; } = string.Empty;
        public int Experience { get; set; }
        public double Rating { get; set; }
        public string Status { get; set; } = string.Empty;
        public string TeachingMethod { get; set; } = string.Empty;
        public string VoiceStyle { get; set; } = string.Empty;
        public string About { get; set; } = string.Empty;

        public User User { get; set; } = null!;
        public ICollection<MentorSchedule> MentorSchedules { get; set; } = new List<MentorSchedule>();
        public ICollection<ContentLibrary> ContentLibraries { get; set; } = new List<ContentLibrary>();
        public ICollection<TeachingCertificate> TeachingCertificates { get; set; } = new List<TeachingCertificate>();
    }
}
