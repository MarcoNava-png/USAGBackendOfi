namespace WebApplication2.Core.DTOs.Permission
{
    public class ModulePermissionsDto
    {
        public string Module { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
