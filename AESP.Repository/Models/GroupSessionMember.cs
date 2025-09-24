using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class GroupSessionMember
    {
        [Key]
        public Guid GroupSessionMemberId { get; set; }
        public DateTime JoinedTime { get; set; }
        public string RoleInSession { get; set; } = string.Empty;

        public DateTime LeaveTime { get; set; }


        public Guid GroupSessionId { get; set; }
        [ForeignKey(nameof(GroupSessionId))]
        public GroupSession GroupSession { get; set; } = null!;


        public Guid LearnerProfileId { get; set; }
        [ForeignKey(nameof(LearnerProfileId))]
        public LearnerProfile LearnerProfile { get; set; } = null!;
    }
}
