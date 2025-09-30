using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESP.Repository.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string? DeviceInfo { get; set; }

        public DateTime ExpiredAt { get; set; }

        [MaxLength(255)]
        public string? IpAddress { get; set; }

        public bool Revoked { get; set; } = false;

        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
