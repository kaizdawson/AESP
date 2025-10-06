using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ReviewerProfile
    {
        [Key]
        public Guid ReviewerProfileId { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public string? Experience { get; set; }
        public double Rating { get; set; }
        public string Status { get; set; }
        public string Levels { get; set; }

        [ForeignKey("Wallet")]
        public Guid WalletId { get; set; }
        public virtual Wallet Wallet { get; set; }


        public virtual ICollection<Certificate> Certificates { get; set; }

        public virtual ICollection<Review> Reviews { get; set; }

        public virtual User User { get; set; }
    }
}
