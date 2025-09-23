using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Type
    {
        [Key]
        public Guid TypeId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
