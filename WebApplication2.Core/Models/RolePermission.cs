using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Core.Models
{

    [Table("RolePermissions")]
    public class RolePermission
    {
        [Key]
        public int IdRolePermission { get; set; }

        [Required]
        [MaxLength(450)]
        public string RoleId { get; set; } = string.Empty;

        [Required]
        public int PermissionId { get; set; }

        public bool CanView { get; set; } = true;
        public bool CanCreate { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(450)]
        public string? AssignedBy { get; set; }

        [ForeignKey("RoleId")]
        public virtual IdentityRole? Role { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission? Permission { get; set; }
    }
}
