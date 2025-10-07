using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class SubServicePackage
    {
        [Key]
        public Guid SubServicePackageId { get; set; }
        public Guid ServicePackageId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;


        [ForeignKey("ServicePackageId")]
        public virtual ServicePackage ServicePackage { get; set; }
    }
}
