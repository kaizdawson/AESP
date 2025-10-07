using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Certificate
    {
        [Key]
        public Guid CertificateId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        [ForeignKey("ReviewerProfile")]
        public Guid ReviewerProfileId { get; set; }

        public virtual ReviewerProfile ReviewerProfile { get; set; }
    }
}
