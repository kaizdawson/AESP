using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Subscription
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SubscriptionId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? CancelDate { get; set; }

        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;


        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; } = null!;

        public Guid ServicePackageId { get; set; }
        [ForeignKey(nameof(ServicePackageId))]
        public ServicePackage ServicePackage { get; set; } = null!;
        public Purchase? Purchase { get; set; }

    }
}
