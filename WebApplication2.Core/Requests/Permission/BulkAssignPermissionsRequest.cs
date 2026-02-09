namespace WebApplication2.Core.Requests.Permission
{
    public class BulkAssignPermissionsRequest
    {
        public string RoleId { get; set; } = string.Empty;
        public List<PermissionAssignment> Permissions { get; set; } = new();
    }
}
