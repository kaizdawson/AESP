﻿using System.ComponentModel.DataAnnotations;

namespace AESP.Repository.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }       
        public string? RoleName { get; set; }


        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
