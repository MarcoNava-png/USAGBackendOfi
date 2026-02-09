using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Permission;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Permission;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Rol.ROLES_ADMINISTRACION)]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PermissionDto>>> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(permissions);
        }

        [HttpGet("by-module")]
        public async Task<ActionResult<List<ModulePermissionsDto>>> GetPermissionsByModule()
        {
            var permissions = await _permissionService.GetPermissionsByModuleAsync();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionDto>> GetPermission(int id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
            {
                return NotFound();
            }
            return Ok(permission);
        }

        [HttpPost]
        [Authorize(Roles = Rol.SOLO_SUPER_ADMIN)]
        public async Task<ActionResult<Permission>> CreatePermission([FromBody] Permission permission)
        {
            var created = await _permissionService.CreatePermissionAsync(permission);
            return CreatedAtAction(nameof(GetPermission), new { id = created.IdPermission }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Rol.SOLO_SUPER_ADMIN)]
        public async Task<ActionResult<Permission>> UpdatePermission(int id, [FromBody] Permission permission)
        {
            if (id != permission.IdPermission)
            {
                return BadRequest("El ID no coincide");
            }
            var updated = await _permissionService.UpdatePermissionAsync(permission);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Rol.SOLO_SUPER_ADMIN)]
        public async Task<ActionResult> DeletePermission(int id)
        {
            await _permissionService.DeletePermissionAsync(id);
            return NoContent();
        }


        [HttpGet("roles")]
        public async Task<ActionResult<List<RoleWithPermissionsDto>>> GetAllRolesWithPermissions()
        {
            var roles = await _permissionService.GetAllRolesWithPermissionsAsync();
            return Ok(roles);
        }

        [HttpGet("roles/{roleId}")]
        public async Task<ActionResult<RoleWithPermissionsDto>> GetRolePermissions(string roleId)
        {
            var role = await _permissionService.GetRolePermissionsAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        [HttpGet("roles/by-name/{roleName}")]
        public async Task<ActionResult<List<RolePermissionDto>>> GetPermissionsByRoleName(string roleName)
        {
            var permissions = await _permissionService.GetPermissionsByRoleNameAsync(roleName);
            return Ok(permissions);
        }


        [HttpPost("assign")]
        public async Task<ActionResult<RolePermission>> AssignPermissionToRole([FromBody] AssignPermissionRequest request)
        {
            if (await EsRolAdminAsync(request.RoleId) && !User.IsInRole(Rol.SUPER_ADMIN))
            {
                return Forbid("Solo el Super Admin puede modificar permisos del rol admin");
            }

            var userId = User.FindFirst("userId")?.Value;
            var rolePermission = await _permissionService.AssignPermissionToRoleAsync(request, userId);
            return Ok(rolePermission);
        }

        [HttpPost("assign-bulk")]
        public async Task<ActionResult> BulkAssignPermissions([FromBody] BulkAssignPermissionsRequest request)
        {
            if (await EsRolAdminAsync(request.RoleId) && !User.IsInRole(Rol.SUPER_ADMIN))
            {
                return Forbid("Solo el Super Admin puede modificar permisos del rol admin");
            }

            var userId = User.FindFirst("userId")?.Value;
            await _permissionService.BulkAssignPermissionsToRoleAsync(request, userId);
            return Ok(new { message = "Permisos asignados correctamente" });
        }

        [HttpDelete("roles/{roleId}/permissions/{permissionId}")]
        public async Task<ActionResult> RemovePermissionFromRole(string roleId, int permissionId)
        {
            if (await EsRolAdminAsync(roleId) && !User.IsInRole(Rol.SUPER_ADMIN))
            {
                return Forbid("Solo el Super Admin puede modificar permisos del rol admin");
            }

            await _permissionService.RemovePermissionFromRoleAsync(roleId, permissionId);
            return NoContent();
        }

        [HttpDelete("roles/{roleId}/permissions")]
        [Authorize(Roles = Rol.SOLO_SUPER_ADMIN)]
        public async Task<ActionResult> RemoveAllPermissionsFromRole(string roleId)
        {
            await _permissionService.RemoveAllPermissionsFromRoleAsync(roleId);
            return NoContent();
        }

        private async Task<bool> EsRolAdminAsync(string roleId)
        {
            var roleName = await _permissionService.GetRoleNameByIdAsync(roleId);
            return roleName?.ToLower() == Rol.ADMIN || roleName?.ToLower() == Rol.SUPER_ADMIN;
        }


        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<UserPermissionsDto>> GetUserPermissions(string userId)
        {
            var currentUserId = User.FindFirst("userId")?.Value;
            var isAdminOrSuperAdmin = User.IsInRole(Rol.ADMIN) || User.IsInRole(Rol.SUPER_ADMIN);

            if (currentUserId != userId && !isAdminOrSuperAdmin)
            {
                return Forbid();
            }

            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }

        [HttpGet("user/{userId}/modules")]
        [Authorize]
        public async Task<ActionResult<List<string>>> GetUserModules(string userId)
        {
            var currentUserId = User.FindFirst("userId")?.Value;
            var isAdminOrSuperAdmin = User.IsInRole(Rol.ADMIN) || User.IsInRole(Rol.SUPER_ADMIN);

            if (currentUserId != userId && !isAdminOrSuperAdmin)
            {
                return Forbid();
            }

            var modules = await _permissionService.GetUserModulesAsync(userId);
            return Ok(modules);
        }

        [HttpGet("check")]
        [Authorize]
        public async Task<ActionResult<bool>> CheckPermission([FromQuery] string permissionCode, [FromQuery] string action = "view")
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var hasPermission = await _permissionService.HasPermissionAsync(userId, permissionCode, action);
            return Ok(hasPermission);
        }

        [HttpGet("my-permissions")]
        [Authorize]
        public async Task<ActionResult<UserPermissionsDto>> GetMyPermissions()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }


        [HttpPost("seed")]
        [Authorize(Roles = Rol.ADMIN)]
        public async Task<ActionResult> RunPermissionSeed(
            [FromServices] Data.DbContexts.ApplicationDbContext context,
            [FromServices] Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager)
        {
            try
            {
                var permissionCount = context.Permissions.Count();
                var rolePermissionCount = context.RolePermissions.Count();

                if (permissionCount > 0)
                {
                    return Ok(new
                    {
                        message = "Los permisos ya existen en la base de datos",
                        permissionCount,
                        rolePermissionCount
                    });
                }

                Data.Seed.PermissionSeed.Seed(context, roleManager);

                var newPermissionCount = context.Permissions.Count();
                var newRolePermissionCount = context.RolePermissions.Count();

                return Ok(new
                {
                    message = "Seed ejecutado correctamente",
                    permissionsCreated = newPermissionCount,
                    rolePermissionsCreated = newRolePermissionCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }
    }
}
