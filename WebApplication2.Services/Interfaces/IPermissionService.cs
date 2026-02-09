using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Permission;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Permission;

namespace WebApplication2.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<List<PermissionDto>> GetAllPermissionsAsync();
        Task<List<ModulePermissionsDto>> GetPermissionsByModuleAsync();
        Task<PermissionDto?> GetPermissionByIdAsync(int id);
        Task<PermissionDto?> GetPermissionByCodeAsync(string code);
        Task<Permission> CreatePermissionAsync(Permission permission);
        Task<Permission> UpdatePermissionAsync(Permission permission);
        Task DeletePermissionAsync(int id);

        Task<List<RoleWithPermissionsDto>> GetAllRolesWithPermissionsAsync();
        Task<RoleWithPermissionsDto?> GetRolePermissionsAsync(string roleId);
        Task<List<RolePermissionDto>> GetPermissionsByRoleNameAsync(string roleName);
        Task<string?> GetRoleNameByIdAsync(string roleId);

        Task<RolePermission> AssignPermissionToRoleAsync(AssignPermissionRequest request, string? assignedBy = null);
        Task BulkAssignPermissionsToRoleAsync(BulkAssignPermissionsRequest request, string? assignedBy = null);
        Task RemovePermissionFromRoleAsync(string roleId, int permissionId);
        Task RemoveAllPermissionsFromRoleAsync(string roleId);

        Task<bool> HasPermissionAsync(string userId, string permissionCode, string action = "view");
        Task<UserPermissionsDto> GetUserPermissionsAsync(string userId);
        Task<List<string>> GetUserModulesAsync(string userId);
    }
}
