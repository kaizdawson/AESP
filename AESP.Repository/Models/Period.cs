using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Period
    {
        [Key]
        public Guid PeriodId { get; set; }
        public string Name { get; set; }



        public ICollection<Report> Reports { get; set; }
    }
}
