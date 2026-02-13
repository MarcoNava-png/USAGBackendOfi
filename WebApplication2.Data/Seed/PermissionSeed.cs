using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Data.Seed
{
    public static class PermissionSeed
    {
        public static void Seed(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            Console.WriteLine("=== INICIANDO PermissionSeed ===");

            if (!context.Permissions.Any())
            {
                Console.WriteLine("No hay permisos, insertando datos iniciales...");
                var permissions = GetInitialPermissions();
                Console.WriteLine($"Insertando {permissions.Count} permisos...");
                context.Permissions.AddRange(permissions);
                context.SaveChanges();
                Console.WriteLine("Permisos insertados correctamente.");
            }
            else
            {
                Console.WriteLine($"Ya existen {context.Permissions.Count()} permisos en la BD.");
            }

            Console.WriteLine("Asignando permisos a roles...");
            AssignPermissionsToRoles(context, roleManager);
            Console.WriteLine("=== PermissionSeed COMPLETADO ===");
        }

        private static List<Permission> GetInitialPermissions()
        {
            return new List<Permission>
            {
                new Permission { Code = "dashboard.view", Name = "Ver Dashboard", Description = "Acceso al panel principal", Module = "Dashboard" },

                new Permission { Code = "aspirantes.view", Name = "Ver Aspirantes", Description = "Ver listado de aspirantes", Module = "Admisiones" },
                new Permission { Code = "aspirantes.create", Name = "Crear Aspirantes", Description = "Registrar nuevos aspirantes", Module = "Admisiones" },
                new Permission { Code = "aspirantes.edit", Name = "Editar Aspirantes", Description = "Modificar datos de aspirantes", Module = "Admisiones" },
                new Permission { Code = "aspirantes.delete", Name = "Eliminar Aspirantes", Description = "Eliminar aspirantes", Module = "Admisiones" },
                new Permission { Code = "aspirantes.inscribir", Name = "Inscribir Aspirantes", Description = "Convertir aspirante a estudiante", Module = "Admisiones" },

                new Permission { Code = "estudiantes.view", Name = "Ver Estudiantes", Description = "Ver listado de estudiantes", Module = "Estudiantes" },
                new Permission { Code = "estudiantes.create", Name = "Crear Estudiantes", Description = "Registrar nuevos estudiantes", Module = "Estudiantes" },
                new Permission { Code = "estudiantes.edit", Name = "Editar Estudiantes", Description = "Modificar datos de estudiantes", Module = "Estudiantes" },
                new Permission { Code = "estudiantes.delete", Name = "Eliminar Estudiantes", Description = "Eliminar estudiantes", Module = "Estudiantes" },

                new Permission { Code = "campus.view", Name = "Ver Campus", Description = "Ver listado de campus", Module = "Catalogos" },
                new Permission { Code = "campus.manage", Name = "Gestionar Campus", Description = "Crear, editar y eliminar campus", Module = "Catalogos" },
                new Permission { Code = "materias.view", Name = "Ver Materias", Description = "Ver listado de materias", Module = "Catalogos" },
                new Permission { Code = "materias.manage", Name = "Gestionar Materias", Description = "Crear, editar y eliminar materias", Module = "Catalogos" },
                new Permission { Code = "planes.view", Name = "Ver Planes de Estudio", Description = "Ver planes de estudio", Module = "Catalogos" },
                new Permission { Code = "planes.manage", Name = "Gestionar Planes de Estudio", Description = "Crear, editar y eliminar planes", Module = "Catalogos" },
                new Permission { Code = "periodos.view", Name = "Ver Periodos", Description = "Ver periodos academicos", Module = "Catalogos" },
                new Permission { Code = "periodos.manage", Name = "Gestionar Periodos", Description = "Crear, editar y eliminar periodos", Module = "Catalogos" },

                // Academico
                new Permission { Code = "grupos.view", Name = "Ver Grupos", Description = "Ver listado de grupos", Module = "Academico" },
                new Permission { Code = "grupos.manage", Name = "Gestionar Grupos", Description = "Crear, editar y eliminar grupos", Module = "Academico" },
                new Permission { Code = "horarios.view", Name = "Ver Horarios", Description = "Ver horarios", Module = "Academico" },
                new Permission { Code = "horarios.manage", Name = "Gestionar Horarios", Description = "Crear, editar y eliminar horarios", Module = "Academico" },
                new Permission { Code = "profesores.view", Name = "Ver Profesores", Description = "Ver listado de profesores", Module = "Academico" },
                new Permission { Code = "profesores.manage", Name = "Gestionar Profesores", Description = "Crear, editar y eliminar profesores", Module = "Academico" },
                new Permission { Code = "calificaciones.view", Name = "Ver Calificaciones", Description = "Ver calificaciones", Module = "Academico" },
                new Permission { Code = "calificaciones.manage", Name = "Gestionar Calificaciones", Description = "Registrar y editar calificaciones", Module = "Academico" },
                new Permission { Code = "asistencia.view", Name = "Ver Asistencia", Description = "Ver registros de asistencia", Module = "Academico" },
                new Permission { Code = "asistencia.manage", Name = "Gestionar Asistencia", Description = "Registrar asistencia", Module = "Academico" },

                // Finanzas
                new Permission { Code = "caja.view", Name = "Ver Caja", Description = "Ver cortes de caja", Module = "Finanzas" },
                new Permission { Code = "caja.manage", Name = "Gestionar Caja", Description = "Abrir/cerrar caja, registrar pagos", Module = "Finanzas" },
                new Permission { Code = "recibos.view", Name = "Ver Recibos", Description = "Ver recibos de pago", Module = "Finanzas" },
                new Permission { Code = "recibos.manage", Name = "Gestionar Recibos", Description = "Crear y anular recibos", Module = "Finanzas" },
                new Permission { Code = "pagos.view", Name = "Ver Pagos", Description = "Ver historial de pagos", Module = "Finanzas" },
                new Permission { Code = "pagos.manage", Name = "Gestionar Pagos", Description = "Registrar y aplicar pagos", Module = "Finanzas" },
                new Permission { Code = "conceptos.view", Name = "Ver Conceptos de Pago", Description = "Ver conceptos de pago", Module = "Finanzas" },
                new Permission { Code = "conceptos.manage", Name = "Gestionar Conceptos", Description = "Crear, editar y eliminar conceptos", Module = "Finanzas" },
                new Permission { Code = "becas.view", Name = "Ver Becas", Description = "Ver becas asignadas", Module = "Finanzas" },
                new Permission { Code = "becas.manage", Name = "Gestionar Becas", Description = "Asignar y modificar becas", Module = "Finanzas" },

                // Configuracion
                new Permission { Code = "usuarios.view", Name = "Ver Usuarios", Description = "Ver listado de usuarios", Module = "Configuracion" },
                new Permission { Code = "usuarios.manage", Name = "Gestionar Usuarios", Description = "Crear, editar y eliminar usuarios", Module = "Configuracion" },
                new Permission { Code = "roles.view", Name = "Ver Roles", Description = "Ver roles del sistema", Module = "Configuracion" },
                new Permission { Code = "roles.manage", Name = "Gestionar Roles", Description = "Configurar permisos de roles", Module = "Configuracion" },
                new Permission { Code = "permisos.view", Name = "Ver Permisos", Description = "Ver permisos del sistema", Module = "Configuracion" },
                new Permission { Code = "permisos.manage", Name = "Gestionar Permisos", Description = "Asignar permisos a roles", Module = "Configuracion" },

                new Permission { Code = "sistema.auditoria", Name = "Ver Auditoría", Description = "Ver registros de auditoría del sistema", Module = "Sistema" },
                new Permission { Code = "sistema.backup", Name = "Gestionar Backups", Description = "Crear y restaurar backups", Module = "Sistema" },
                new Permission { Code = "sistema.configuracion", Name = "Configuración Avanzada", Description = "Configuración avanzada del sistema", Module = "Sistema" },
                new Permission { Code = "admin.manage", Name = "Gestionar Administradores", Description = "Crear, editar y eliminar usuarios admin", Module = "Sistema" },
            };
        }

        private static void AssignPermissionsToRoles(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            var permissions = context.Permissions.ToList();
            var roles = roleManager.Roles.ToList();

            foreach (var role in roles)
            {
                var existingAssignments = context.RolePermissions
                    .Where(rp => rp.RoleId == role.Id)
                    .Select(rp => rp.PermissionId)
                    .ToList();

                var permissionsToAssign = GetPermissionsForRole(role.Name ?? "", permissions)
                    .Where(p => !existingAssignments.Contains(p.permission.IdPermission))
                    .ToList();

                foreach (var (permission, canView, canCreate, canEdit, canDelete) in permissionsToAssign)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.IdPermission,
                        CanView = canView,
                        CanCreate = canCreate,
                        CanEdit = canEdit,
                        CanDelete = canDelete,
                        AssignedBy = "System"
                    });
                }
            }

            context.SaveChanges();
        }

        private static List<(Permission permission, bool canView, bool canCreate, bool canEdit, bool canDelete)>
            GetPermissionsForRole(string roleName, List<Permission> allPermissions)
        {
            var result = new List<(Permission, bool, bool, bool, bool)>();

            switch (roleName.ToLower())
            {
                case Rol.SUPER_ADMIN:

                    foreach (var p in allPermissions)
                    {
                        result.Add((p, true, true, true, true));
                    }
                    break;

                case Rol.ADMIN:
                    foreach (var p in allPermissions.Where(p => p.Module != "Sistema"))
                    {
                        result.Add((p, true, true, true, true));
                    }
                    break;

                case Rol.DIRECTOR:
                    foreach (var p in allPermissions)
                    {
                        var canModify = p.Module != "Configuracion";
                        result.Add((p, true, canModify, canModify, false));
                    }
                    break;

                case Rol.COORDINADOR:
                    var coordModules = new[] { "Dashboard", "Academico", "Estudiantes", "Catalogos" };
                    foreach (var p in allPermissions.Where(p => coordModules.Contains(p.Module)))
                    {
                        var canModify = p.Module == "Academico" || p.Module == "Estudiantes";
                        result.Add((p, true, canModify, canModify, false));
                    }
                    break;

                case Rol.CONTROL_ESCOLAR:
                    var ceModules = new[] { "Dashboard", "Admisiones", "Estudiantes", "Finanzas", "Catalogos" };
                    foreach (var p in allPermissions.Where(p => ceModules.Contains(p.Module)))
                    {
                        result.Add((p, true, true, true, p.Module != "Catalogos"));
                    }
                    break;

                case Rol.DOCENTE:
                    var docentePermissions = new[] { "dashboard.view", "grupos.view", "calificaciones.view",
                        "calificaciones.manage", "asistencia.view", "asistencia.manage", "estudiantes.view" };
                    foreach (var p in allPermissions.Where(p => docentePermissions.Contains(p.Code)))
                    {
                        var canModify = p.Code.Contains("calificaciones") || p.Code.Contains("asistencia");
                        result.Add((p, true, canModify, canModify, false));
                    }
                    break;

                case Rol.ALUMNO:
                    var alumnoPermissions = new[] { "dashboard.view" };
                    foreach (var p in allPermissions.Where(p => alumnoPermissions.Contains(p.Code)))
                    {
                        result.Add((p, true, false, false, false));
                    }
                    break;

                case Rol.FINANZAS:
                    var finanzasModules = new[] { "Dashboard", "Finanzas" };
                    var finanzasViewOnly = new[] { "Estudiantes", "Admisiones" }; 
                    foreach (var p in allPermissions.Where(p => finanzasModules.Contains(p.Module)))
                    {
                        result.Add((p, true, true, true, p.Code.Contains("conceptos") || p.Code.Contains("becas")));
                    }
                    foreach (var p in allPermissions.Where(p => finanzasViewOnly.Contains(p.Module)))
                    {
                        result.Add((p, true, false, false, false));
                    }
                    break;

                case Rol.ACADEMICO:
                    var academicoFullModules = new[] { "Dashboard", "Academico", "Estudiantes", "Catalogos" };
                    var academicoViewOnly = new[] { "Admisiones" };
                    foreach (var p in allPermissions.Where(p => academicoFullModules.Contains(p.Module)))
                    {
                        result.Add((p, true, true, true, true));
                    }
                    foreach (var p in allPermissions.Where(p => academicoViewOnly.Contains(p.Module)))
                    {
                        result.Add((p, true, false, false, false));
                    }
                    break;

                case Rol.ADMISIONES:
                    var admisionesModulesTotal = new[] { "Dashboard", "Admisiones" };
                    var admisionesViewFinanzas = new[] { "Finanzas" }; 
                    foreach (var p in allPermissions.Where(p => admisionesModulesTotal.Contains(p.Module)))
                    {
                        result.Add((p, true, true, true, false));
                    }
                    foreach (var p in allPermissions.Where(p => admisionesViewFinanzas.Contains(p.Module)))
                    {
                        var canManage = p.Code == "caja.manage" || p.Code == "pagos.manage" || p.Code == "recibos.view";
                        result.Add((p, true, canManage, canManage, false));
                    }
                    break;
            }

            return result;
        }
    }
}
