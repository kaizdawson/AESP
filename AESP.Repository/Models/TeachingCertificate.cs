using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class TeachingCertificate
    {
        [Key]
        public Guid TeachingCertificateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public Guid MentorProfileId { get; set; }
        [ForeignKey(nameof(MentorProfileId))]
        public MentorProfile MentorProfile { get; set; } = null!;
    }
}
