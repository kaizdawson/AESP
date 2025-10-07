using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Wallet
    {
        [Key]
        public Guid WalletId { get; set; }
        public double Amount { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
    }
}
