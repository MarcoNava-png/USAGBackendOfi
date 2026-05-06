using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.AccesoAlumnoDocente;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services;

public class AccesoAlumnoDocenteService : IAccesoAlumnoDocenteService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccesoAlumnoDocenteService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<AccesosListaDto> ListarAsync(string tipo, string? busqueda, int pagina, int tamanoPagina, CancellationToken ct = default)
    {
        var esDocente = tipo.Equals("docente", StringComparison.OrdinalIgnoreCase);
        return esDocente
            ? await ListarDocentesAsync(busqueda, pagina, tamanoPagina, ct)
            : await ListarAlumnosAsync(busqueda, pagina, tamanoPagina, ct);
    }

    private async Task<AccesosListaDto> ListarAlumnosAsync(string? busqueda, int pagina, int tamanoPagina, CancellationToken ct)
    {
        var query = _db.Estudiante
            .AsNoTracking()
            .Where(e => e.Activo)
            .Include(e => e.IdPersonaNavigation)
            .Include(e => e.IdPlanActualNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.Trim().ToLower();
            query = query.Where(e =>
                e.Matricula.ToLower().Contains(b) ||
                (e.Email ?? "").ToLower().Contains(b) ||
                (e.IdPersonaNavigation.Nombre ?? "").ToLower().Contains(b) ||
                (e.IdPersonaNavigation.ApellidoPaterno ?? "").ToLower().Contains(b) ||
                (e.IdPersonaNavigation.ApellidoMaterno ?? "").ToLower().Contains(b));
        }

        var total = await query.CountAsync(ct);

        var estudiantes = await query
            .OrderBy(e => e.IdPersonaNavigation.ApellidoPaterno)
            .ThenBy(e => e.IdPersonaNavigation.Nombre)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(e => new
            {
                e.IdEstudiante,
                e.Matricula,
                e.UsuarioId,
                e.Email,
                e.Activo,
                Nombre = e.IdPersonaNavigation.Nombre,
                ApePa = e.IdPersonaNavigation.ApellidoPaterno,
                ApeMa = e.IdPersonaNavigation.ApellidoMaterno,
                Plan = e.IdPlanActualNavigation != null ? e.IdPlanActualNavigation.NombrePlanEstudios : null
            })
            .ToListAsync(ct);

        var userIds = estudiantes.Where(x => x.UsuarioId != null).Select(x => x.UsuarioId!).ToList();
        var users = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.UserName, u.MustChangePassword, u.AccessFailedCount, u.LockoutEnd })
            .ToListAsync(ct);

        var idsEstudiantes = estudiantes.Select(x => x.IdEstudiante).ToList();
        var grupos = await _db.Inscripcion
            .AsNoTracking()
            .Where(i => idsEstudiantes.Contains(i.IdEstudiante) && i.Estado == "Inscrito")
            .Include(i => i.IdGrupoMateriaNavigation!)
                .ThenInclude(gm => gm.IdGrupoNavigation!)
                    .ThenInclude(g => g.IdPeriodoAcademicoNavigation)
            .Where(i => i.IdGrupoMateriaNavigation!.IdGrupoNavigation!.IdPeriodoAcademicoNavigation!.EsPeriodoActual)
            .Select(i => new
            {
                i.IdEstudiante,
                CodigoGrupo = i.IdGrupoMateriaNavigation!.IdGrupoNavigation!.CodigoGrupo,
                Cuatrimestre = (int)i.IdGrupoMateriaNavigation.IdGrupoNavigation.NumeroCuatrimestre
            })
            .ToListAsync(ct);

        var grupoPorEstudiante = grupos.GroupBy(g => g.IdEstudiante).ToDictionary(g => g.Key, g => g.First());

        var items = new List<AccesoUsuarioDto>();
        foreach (var e in estudiantes)
        {
            var user = e.UsuarioId != null ? users.FirstOrDefault(u => u.Id == e.UsuarioId) : null;
            var grp = grupoPorEstudiante.TryGetValue(e.IdEstudiante, out var gInfo) ? gInfo : null;

            items.Add(new AccesoUsuarioDto
            {
                UserId = user?.Id,
                IdEstudiante = e.IdEstudiante,
                Email = user?.Email ?? e.Email ?? string.Empty,
                UserName = user?.UserName,
                NombreCompleto = $"{e.Nombre} {e.ApePa} {e.ApeMa}".Trim(),
                Rol = Rol.ALUMNO,
                Matricula = e.Matricula,
                PlanEstudios = e.Plan,
                GrupoActual = grp?.CodigoGrupo,
                Cuatrimestre = grp?.Cuatrimestre,
                TieneCuenta = user != null,
                CuentaBloqueada = user?.LockoutEnd.HasValue == true && user.LockoutEnd!.Value > DateTimeOffset.UtcNow,
                BloqueadaHasta = user?.LockoutEnd?.UtcDateTime,
                IntentosFallidos = user?.AccessFailedCount ?? 0,
                DebeCambiarPassword = user?.MustChangePassword ?? false,
                NuncaHaIngresado = user?.MustChangePassword ?? (user == null),
                Activo = e.Activo
            });
        }

        return new AccesosListaDto
        {
            Total = total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            Items = items
        };
    }

    private async Task<AccesosListaDto> ListarDocentesAsync(string? busqueda, int pagina, int tamanoPagina, CancellationToken ct)
    {
        var query = _db.Profesor
            .AsNoTracking()
            .Where(p => p.Activo)
            .Include(p => p.IdPersonaNavigation)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.Trim().ToLower();
            query = query.Where(p =>
                p.NoEmpleado.ToLower().Contains(b) ||
                (p.EmailInstitucional ?? "").ToLower().Contains(b) ||
                (p.IdPersonaNavigation.Nombre ?? "").ToLower().Contains(b) ||
                (p.IdPersonaNavigation.ApellidoPaterno ?? "").ToLower().Contains(b) ||
                (p.IdPersonaNavigation.ApellidoMaterno ?? "").ToLower().Contains(b));
        }

        var total = await query.CountAsync(ct);

        var profesores = await query
            .OrderBy(p => p.IdPersonaNavigation.ApellidoPaterno)
            .ThenBy(p => p.IdPersonaNavigation.Nombre)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .Select(p => new
            {
                p.IdProfesor,
                p.NoEmpleado,
                p.UsuarioId,
                p.EmailInstitucional,
                p.Activo,
                Nombre = p.IdPersonaNavigation.Nombre,
                ApePa = p.IdPersonaNavigation.ApellidoPaterno,
                ApeMa = p.IdPersonaNavigation.ApellidoMaterno,
            })
            .ToListAsync(ct);

        var userIds = profesores.Where(x => x.UsuarioId != null).Select(x => x.UsuarioId!).ToList();
        var users = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.UserName, u.MustChangePassword, u.AccessFailedCount, u.LockoutEnd })
            .ToListAsync(ct);

        var items = profesores.Select(p =>
        {
            var user = p.UsuarioId != null ? users.FirstOrDefault(u => u.Id == p.UsuarioId) : null;
            return new AccesoUsuarioDto
            {
                UserId = user?.Id,
                IdProfesor = p.IdProfesor,
                Email = user?.Email ?? p.EmailInstitucional ?? string.Empty,
                UserName = user?.UserName,
                NombreCompleto = $"{p.Nombre} {p.ApePa} {p.ApeMa}".Trim(),
                Rol = Rol.DOCENTE,
                Matricula = p.NoEmpleado,
                TieneCuenta = user != null,
                CuentaBloqueada = user?.LockoutEnd.HasValue == true && user.LockoutEnd!.Value > DateTimeOffset.UtcNow,
                BloqueadaHasta = user?.LockoutEnd?.UtcDateTime,
                IntentosFallidos = user?.AccessFailedCount ?? 0,
                DebeCambiarPassword = user?.MustChangePassword ?? false,
                NuncaHaIngresado = user?.MustChangePassword ?? (user == null),
                Activo = p.Activo
            };
        }).ToList();

        return new AccesosListaDto
        {
            Total = total,
            Pagina = pagina,
            TamanoPagina = tamanoPagina,
            Items = items
        };
    }

    public async Task<ResetearPasswordResponse> ResetearPasswordAsync(ResetearPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return new ResetearPasswordResponse { Exito = false, Mensaje = "Usuario no encontrado" };

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(Rol.ALUMNO) && !roles.Contains(Rol.DOCENTE))
            return new ResetearPasswordResponse { Exito = false, Mensaje = "Este endpoint es exclusivo para alumnos y docentes" };

        var passwordTemporal = !string.IsNullOrWhiteSpace(request.NuevaPassword)
            ? request.NuevaPassword
            : GenerarPasswordTemporal();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, passwordTemporal);

        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            return new ResetearPasswordResponse { Exito = false, Mensaje = errors };
        }

        user.MustChangePassword = request.ForzarCambio;
        await _userManager.UpdateAsync(user);
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);

        return new ResetearPasswordResponse
        {
            Exito = true,
            PasswordTemporal = passwordTemporal,
            Mensaje = "Contraseña restablecida"
        };
    }

    public async Task<bool> DesbloquearCuentaAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(Rol.ALUMNO) && !roles.Contains(Rol.DOCENTE))
            return false;

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);
        return true;
    }

    public async Task<ResetearPasswordResponse> CrearAccesoAsync(CrearAccesoRequest request, CancellationToken ct = default)
    {
        var esDocente = request.Tipo.Equals("docente", StringComparison.OrdinalIgnoreCase);
        var rolAsignar = esDocente ? Rol.DOCENTE : Rol.ALUMNO;

        string? email = request.EmailPersonalizado?.Trim();
        string? userName = null;
        string nombres = string.Empty, apellidos = string.Empty;
        Estudiante? estudiante = null;
        Profesor? profesor = null;

        if (esDocente)
        {
            profesor = await _db.Profesor
                .Include(p => p.IdPersonaNavigation)
                .FirstOrDefaultAsync(p => p.IdProfesor == request.EntidadId, ct);
            if (profesor == null)
                return new ResetearPasswordResponse { Exito = false, Mensaje = "Docente no encontrado" };
            if (!string.IsNullOrEmpty(profesor.UsuarioId))
                return new ResetearPasswordResponse { Exito = false, Mensaje = "Este docente ya tiene cuenta de acceso" };

            if (string.IsNullOrWhiteSpace(email)) email = profesor.EmailInstitucional;
            if (string.IsNullOrWhiteSpace(email))
                return new ResetearPasswordResponse { Exito = false, Mensaje = "El docente no tiene email y no se proporcionó uno" };
            userName = email;
            nombres = profesor.IdPersonaNavigation?.Nombre ?? string.Empty;
            apellidos = $"{profesor.IdPersonaNavigation?.ApellidoPaterno} {profesor.IdPersonaNavigation?.ApellidoMaterno}".Trim();
        }
        else
        {
            estudiante = await _db.Estudiante
                .Include(e => e.IdPersonaNavigation)
                .FirstOrDefaultAsync(e => e.IdEstudiante == request.EntidadId, ct);
            if (estudiante == null)
                return new ResetearPasswordResponse { Exito = false, Mensaje = "Estudiante no encontrado" };
            if (!string.IsNullOrEmpty(estudiante.UsuarioId))
                return new ResetearPasswordResponse { Exito = false, Mensaje = "Este estudiante ya tiene cuenta de acceso" };

            if (string.IsNullOrWhiteSpace(email))
                email = !string.IsNullOrWhiteSpace(estudiante.Email) ? estudiante.Email : $"{estudiante.Matricula}@usaguanajuato.edu.mx";
            userName = email;
            nombres = estudiante.IdPersonaNavigation?.Nombre ?? string.Empty;
            apellidos = $"{estudiante.IdPersonaNavigation?.ApellidoPaterno} {estudiante.IdPersonaNavigation?.ApellidoMaterno}".Trim();
        }

        var existente = await _userManager.FindByEmailAsync(email!);
        if (existente != null)
            return new ResetearPasswordResponse { Exito = false, Mensaje = $"Ya existe un usuario con el email {email}" };

        var passwordTemporal = !string.IsNullOrWhiteSpace(request.PasswordPersonalizada)
            ? request.PasswordPersonalizada!
            : GenerarPasswordTemporal();

        var nuevoUser = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            Nombres = nombres,
            Apellidos = apellidos,
            MustChangePassword = true,
            LockoutEnabled = true
        };

        var creado = await _userManager.CreateAsync(nuevoUser, passwordTemporal);
        if (!creado.Succeeded)
        {
            var errs = string.Join(" ", creado.Errors.Select(e => e.Description));
            return new ResetearPasswordResponse { Exito = false, Mensaje = errs };
        }

        var rolRes = await _userManager.AddToRoleAsync(nuevoUser, rolAsignar);
        if (!rolRes.Succeeded)
        {
            await _userManager.DeleteAsync(nuevoUser);
            return new ResetearPasswordResponse { Exito = false, Mensaje = "No se pudo asignar el rol" };
        }

        if (esDocente && profesor != null)
        {
            profesor.UsuarioId = nuevoUser.Id;
            await _db.SaveChangesAsync(ct);
        }
        else if (estudiante != null)
        {
            estudiante.UsuarioId = nuevoUser.Id;
            if (string.IsNullOrWhiteSpace(estudiante.Email)) estudiante.Email = email;
            await _db.SaveChangesAsync(ct);
        }

        return new ResetearPasswordResponse
        {
            Exito = true,
            PasswordTemporal = passwordTemporal,
            Mensaje = $"Acceso creado con email {email}"
        };
    }

    private static string GenerarPasswordTemporal()
    {
        const string mayus = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string minus = "abcdefghjkmnpqrstuvwxyz";
        const string nums = "23456789";
        const string simbolo = "*!@#$";
        var rng = Random.Shared;
        var chars = new List<char>
        {
            mayus[rng.Next(mayus.Length)],
            mayus[rng.Next(mayus.Length)],
            nums[rng.Next(nums.Length)],
            nums[rng.Next(nums.Length)],
            nums[rng.Next(nums.Length)],
            simbolo[rng.Next(simbolo.Length)],
            simbolo[rng.Next(simbolo.Length)]
        };
        for (int i = 0; i < 7; i++) chars.Add(minus[rng.Next(minus.Length)]);
        return new string(chars.OrderBy(_ => rng.Next()).ToArray());
    }
}
