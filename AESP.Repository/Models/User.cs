using System.Data;

namespace AESP.Repository.Models
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string Status { get; set; } = "Active";

        public Role? Role { get; set; }
    }
}
