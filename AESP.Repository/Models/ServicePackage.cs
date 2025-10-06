using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ServicePackage
    {
        [Key]
        public Guid ServicePackageId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Subscription> Subscriptions { get; set; }
        public virtual ICollection<SubServicePackage> SubServicePackages { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
    }
}
