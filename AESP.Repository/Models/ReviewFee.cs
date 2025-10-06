using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class ReviewFee
    {
        [Key]
        public Guid ReviewFeeId { get; set; }
        public double Price { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; }

    }
}
