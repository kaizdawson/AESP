using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Common.DTOs
{
    public class UpdateReviewerLevelDto
    {
        [Required(ErrorMessage = "Level không được để trống.")]
        [StringLength(10, ErrorMessage = "Level tối đa 10 ký tự.")]
        public string Level { get; set; } = string.Empty;
    }
}
