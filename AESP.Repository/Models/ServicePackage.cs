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
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Duration { get; set; }
        public bool WithMentor { get; set; }
        public int NumberOfMentorMeeting { get; set; }



        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
