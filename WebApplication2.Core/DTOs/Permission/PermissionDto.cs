namespace WebApplication2.Core.DTOs.Permission
{
    public class PermissionDto
    {
        public int IdPermission { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Module { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
