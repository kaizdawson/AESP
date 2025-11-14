using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AESP.Repository.Models
{
    public class Transaction
    {
        [Key]
        public Guid TransactionId { get; set; } = Guid.NewGuid();

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        [ForeignKey(nameof(ServicePackage))]
        public Guid? ServicePackageId { get; set; }
        public virtual ServicePackage? ServicePackage { get; set; }

   
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountMoney { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountCoin { get; set; }

   
        [MaxLength(50)]
        public string? OrderCode { get; set; }

        [MaxLength(100)]
        public string? BankName { get; set; }

 
        [MaxLength(100)]
        public string? AccountNumber { get; set; }

   
        [MaxLength(500)]
        public string? Description { get; set; }

        
        [MaxLength(50)]
        public string? Type { get; set; }

       
        [MaxLength(50)]
        public string? Status { get; set; }

       
        [MaxLength(300)]
        public string? ReasonReject { get; set; }

       
        public DateTime CreatedTransaction { get; set; } = DateTime.UtcNow;

     

    }
}
