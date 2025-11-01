using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ServicePackage : BaseEntity
    {
        [Key]
        public Guid ServicePackageId { get; set; } = Guid.NewGuid();

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public string Status { get; set; } = "Active";

        public int NumberOfCoin { get; set; } = 0;
        public double BonusPercent { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Navigation ---
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}