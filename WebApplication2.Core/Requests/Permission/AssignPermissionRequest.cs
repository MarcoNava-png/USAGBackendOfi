namespace WebApplication2.Core.Requests.Permission
{
    public class AssignPermissionRequest
    {
        public string RoleId { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        public bool CanView { get; set; } = true;
        public bool CanCreate { get; set; } = false;
        public bool CanEdit { get; set; } = false;
        public bool CanDelete { get; set; } = false;
    }
}
