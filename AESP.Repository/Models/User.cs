using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace AESP.Repository.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public string FirebaseUid { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public string AvatarUrl { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public virtual LearnerProfile LearnerProfile { get; set; }
        public virtual ReviewerProfile ReviewerProfile { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
