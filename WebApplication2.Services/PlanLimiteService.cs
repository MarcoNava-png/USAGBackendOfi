using Microsoft.EntityFrameworkCore;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.MultiTenant;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public interface IPlanLimiteService
    {
        Task<string?> ValidarPuedeCrearUsuarioAsync(CancellationToken ct = default);
        Task<string?> ValidarPuedeCrearEstudianteAsync(int cantidad = 1, CancellationToken ct = default);
        Task<string?> ValidarPuedeCrearCampusAsync(CancellationToken ct = default);
    }

    public class PlanLimiteService : IPlanLimiteService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public PlanLimiteService(ApplicationDbContext db, ITenantContextAccessor tenantContextAccessor)
        {
            _db = db;
            _tenantContextAccessor = tenantContextAccessor;
        }

        public async Task<string?> ValidarPuedeCrearUsuarioAsync(CancellationToken ct = default)
        {
            var max = _tenantContextAccessor.TenantContext?.Settings?.MaxUsuarios ?? 0;
            if (max <= 0)
                return null;

            var actuales = await _db.Users.CountAsync(ct);
            if (actuales >= max)
                return $"Se alcanzó el límite de usuarios del plan de esta escuela ({max}). Contacta al administrador del sistema para ampliar el plan.";

            return null;
        }

        public async Task<string?> ValidarPuedeCrearEstudianteAsync(int cantidad = 1, CancellationToken ct = default)
        {
            var max = _tenantContextAccessor.TenantContext?.Settings?.MaxEstudiantes ?? 0;
            if (max <= 0)
                return null;

            var actuales = await _db.Estudiante.CountAsync(e => e.Activo && e.Status == StatusEnum.Active, ct);
            if (actuales + cantidad > max)
                return $"Se alcanzó el límite de estudiantes del plan de esta escuela ({max}). Contacta al administrador del sistema para ampliar el plan.";

            return null;
        }

        public async Task<string?> ValidarPuedeCrearCampusAsync(CancellationToken ct = default)
        {
            var max = _tenantContextAccessor.TenantContext?.Settings?.MaxCampus ?? 0;
            if (max <= 0)
                return null;

            var actuales = await _db.Campus.CountAsync(c => c.Status == StatusEnum.Active, ct);
            if (actuales >= max)
                return $"Se alcanzó el límite de campus del plan de esta escuela ({max}). Contacta al administrador del sistema para ampliar el plan.";

            return null;
        }
    }
}
