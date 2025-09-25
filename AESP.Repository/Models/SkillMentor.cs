using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class SkillMentor
    {
        [Key]
        public Guid SkillMentorId { get; set; }

        public Guid MentorProfileId { get; set; }
        [ForeignKey(nameof(MentorProfileId))]
        public MentorProfile MentorProfile { get; set; } = null!;

        public Guid SkillId { get; set; }
        [ForeignKey(nameof(SkillId))]
        public Skill Skill { get; set; } = null!;

        public bool IsExpertised { get; set; } = false;
    }
}
