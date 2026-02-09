namespace WebApplication2.Core.DTOs.Permission
{
    public class RoleWithPermissionsDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<RolePermissionDto> Permissions { get; set; } = new();
    }
}
