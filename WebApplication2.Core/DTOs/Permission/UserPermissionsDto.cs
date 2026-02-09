namespace WebApplication2.Core.DTOs.Permission
{
    public class UserPermissionsDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<RolePermissionDto> Permissions { get; set; } = new();
    }
}
