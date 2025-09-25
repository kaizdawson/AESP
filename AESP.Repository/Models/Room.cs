using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class Room
    {
        [Key]
        public Guid RoomId { get; set; }
        public string RoomUrl { get; set; } = string.Empty;

        public ICollection<PracticeSession> PracticeSessions { get; set; } = new List<PracticeSession>();
        public ICollection<GroupSession> GroupSessions { get; set; } = new List<GroupSession>();
    }
}
