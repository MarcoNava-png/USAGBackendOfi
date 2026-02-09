using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Permission;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Permission;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #region Permisos

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            return await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .Select(p => new PermissionDto
                {
                    IdPermission = p.IdPermission,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    Module = p.Module,
                    IsActive = p.IsActive
                })
                .ToListAsync();
        }

        public async Task<List<ModulePermissionsDto>> GetPermissionsByModuleAsync()
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return permissions
                .GroupBy(p => p.Module)
                .Select(g => new ModulePermissionsDto
                {
                    Module = g.Key,
                    Permissions = g.Select(p => new PermissionDto
                    {
                        IdPermission = p.IdPermission,
                        Code = p.Code,
                        Name = p.Name,
                        Description = p.Description,
                        Module = p.Module,
                        IsActive = p.IsActive
                    }).ToList()
                })
                .ToList();
        }

        public async Task<PermissionDto?> GetPermissionByIdAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return null;

            return new PermissionDto
            {
                IdPermission = permission.IdPermission,
                Code = permission.Code,
                Name = permission.Name,
                Description = permission.Description,
                Module = permission.Module,
                IsActive = permission.IsActive
            };
        }

        public async Task<PermissionDto?> GetPermissionByCodeAsync(string code)
        {
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Code == code);

            if (permission == null) return null;

            return new PermissionDto
            {
                IdPermission = permission.IdPermission,
                Code = permission.Code,
                Name = permission.Name,
                Description = permission.Description,
                Module = permission.Module,
                IsActive = permission.IsActive
            };
        }

        public async Task<Permission> CreatePermissionAsync(Permission permission)
        {
            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task<Permission> UpdatePermissionAsync(Permission permission)
        {
            _context.Permissions.Update(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task DeletePermissionAsync(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission != null)
            {
                permission.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Roles

        public async Task<List<RoleWithPermissionsDto>> GetAllRolesWithPermissionsAsync()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var result = new List<RoleWithPermissionsDto>();

            foreach (var role in roles)
            {
                var rolePermissions = await GetRolePermissionsInternalAsync(role.Id, role.Name ?? "");
                result.Add(new RoleWithPermissionsDto
                {
                    RoleId = role.Id,
                    RoleName = role.Name ?? "",
                    Permissions = rolePermissions
                });
            }

            return result;
        }

        public async Task<RoleWithPermissionsDto?> GetRolePermissionsAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return null;

            var permissions = await GetRolePermissionsInternalAsync(roleId, role.Name ?? "");

            return new RoleWithPermissionsDto
            {
                RoleId = role.Id,
                RoleName = role.Name ?? "",
                Permissions = permissions
            };
        }

        public async Task<List<RolePermissionDto>> GetPermissionsByRoleNameAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return new List<RolePermissionDto>();

            return await GetRolePermissionsInternalAsync(role.Id, roleName);
        }

        public async Task<string?> GetRoleNameByIdAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            return role?.Name;
        }

        private async Task<List<RolePermissionDto>> GetRolePermissionsInternalAsync(string roleId, string roleName)
        {
            return await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId && rp.Permission!.IsActive)
                .Select(rp => new RolePermissionDto
                {
                    IdRolePermission = rp.IdRolePermission,
                    RoleId = rp.RoleId,
                    RoleName = roleName,
                    PermissionId = rp.PermissionId,
                    PermissionCode = rp.Permission!.Code,
                    PermissionName = rp.Permission.Name,
                    Module = rp.Permission.Module,
                    CanView = rp.CanView,
                    CanCreate = rp.CanCreate,
                    CanEdit = rp.CanEdit,
                    CanDelete = rp.CanDelete
                })
                .OrderBy(rp => rp.Module)
                .ThenBy(rp => rp.PermissionName)
                .ToListAsync();
        }

        #endregion

        #region Asignacion de permisos

        public async Task<RolePermission> AssignPermissionToRoleAsync(AssignPermissionRequest request, string? assignedBy = null)
        {
            var existing = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == request.RoleId && rp.PermissionId == request.PermissionId);

            if (existing != null)
            {
                existing.CanView = request.CanView;
                existing.CanCreate = request.CanCreate;
                existing.CanEdit = request.CanEdit;
                existing.CanDelete = request.CanDelete;
                existing.AssignedAt = DateTime.UtcNow;
                existing.AssignedBy = assignedBy;
                await _context.SaveChangesAsync();
                return existing;
            }

            var rolePermission = new RolePermission
            {
                RoleId = request.RoleId,
                PermissionId = request.PermissionId,
                CanView = request.CanView,
                CanCreate = request.CanCreate,
                CanEdit = request.CanEdit,
                CanDelete = request.CanDelete,
                AssignedBy = assignedBy
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();
            return rolePermission;
        }

        public async Task BulkAssignPermissionsToRoleAsync(BulkAssignPermissionsRequest request, string? assignedBy = null)
        {
            var existingPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == request.RoleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(existingPermissions);

            foreach (var permission in request.Permissions)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = request.RoleId,
                    PermissionId = permission.PermissionId,
                    CanView = permission.CanView,
                    CanCreate = permission.CanCreate,
                    CanEdit = permission.CanEdit,
                    CanDelete = permission.CanDelete,
                    AssignedBy = assignedBy
                };
                _context.RolePermissions.Add(rolePermission);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemovePermissionFromRoleAsync(string roleId, int permissionId)
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission != null)
            {
                _context.RolePermissions.Remove(rolePermission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveAllPermissionsFromRoleAsync(string roleId)
        {
            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            _context.RolePermissions.RemoveRange(rolePermissions);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Validacion de permisos

        public async Task<bool> HasPermissionAsync(string userId, string permissionCode, string action = "view")
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Any()) return false;

            var roleIds = await _roleManager.Roles
                .Where(r => userRoles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            var rolePermission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .FirstOrDefaultAsync(rp =>
                    roleIds.Contains(rp.RoleId) &&
                    rp.Permission!.Code == permissionCode &&
                    rp.Permission.IsActive);

            if (rolePermission == null) return false;

            return action.ToLower() switch
            {
                "view" => rolePermission.CanView,
                "create" => rolePermission.CanCreate,
                "edit" => rolePermission.CanEdit,
                "delete" => rolePermission.CanDelete,
                _ => rolePermission.CanView
            };
        }

        public async Task<UserPermissionsDto> GetUserPermissionsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new UserPermissionsDto { UserId = userId };
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var roleIds = await _roleManager.Roles
                .Where(r => userRoles.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            var permissions = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Include(rp => rp.Role)
                .Where(rp => roleIds.Contains(rp.RoleId) && rp.Permission!.IsActive)
                .Select(rp => new RolePermissionDto
                {
                    IdRolePermission = rp.IdRolePermission,
                    RoleId = rp.RoleId,
                    RoleName = rp.Role!.Name ?? "",
                    PermissionId = rp.PermissionId,
                    PermissionCode = rp.Permission!.Code,
                    PermissionName = rp.Permission.Name,
                    Module = rp.Permission.Module,
                    CanView = rp.CanView,
                    CanCreate = rp.CanCreate,
                    CanEdit = rp.CanEdit,
                    CanDelete = rp.CanDelete
                })
                .ToListAsync();

            var combinedPermissions = permissions
                .GroupBy(p => p.PermissionCode)
                .Select(g => new RolePermissionDto
                {
                    PermissionId = g.First().PermissionId,
                    PermissionCode = g.Key,
                    PermissionName = g.First().PermissionName,
                    Module = g.First().Module,
                    CanView = g.Any(p => p.CanView),
                    CanCreate = g.Any(p => p.CanCreate),
                    CanEdit = g.Any(p => p.CanEdit),
                    CanDelete = g.Any(p => p.CanDelete)
                })
                .ToList();

            return new UserPermissionsDto
            {
                UserId = userId,
                Email = user.Email ?? "",
                Roles = userRoles.ToList(),
                Permissions = combinedPermissions
            };
        }

        public async Task<List<string>> GetUserModulesAsync(string userId)
        {
            var userPermissions = await GetUserPermissionsAsync(userId);
            return userPermissions.Permissions
                .Where(p => p.CanView)
                .Select(p => p.Module)
                .Distinct()
                .ToList();
        }

        #endregion
    }
}
