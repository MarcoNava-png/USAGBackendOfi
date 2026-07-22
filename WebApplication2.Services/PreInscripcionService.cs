using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.PreInscripcion;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.PreInscripcion;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;
using EstudianteStatusAcademicoEnum = WebApplication2.Core.Enums.EstudianteStatusAcademicoEnum;

namespace WebApplication2.Services
{
    public class PreInscripcionService : IPreInscripcionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IGrupoService _grupoService;

        public PreInscripcionService(ApplicationDbContext dbContext, IGrupoService grupoService)
        {
            _dbContext = dbContext;
            _grupoService = grupoService;
        }

        public async Task<PreInscripcionDto> ApartarParaPeriodoAsync(ApartarPreInscripcionRequest request, CancellationToken ct = default)
        {
            var estudiante = await _dbContext.Estudiante
                .FirstOrDefaultAsync(e => e.IdEstudiante == request.IdEstudiante && e.Status == StatusEnum.Active, ct);

            if (estudiante == null)
                throw new InvalidOperationException("Estudiante no encontrado");

            var planExiste = await _dbContext.PlanEstudios
                .AnyAsync(p => p.IdPlanEstudios == request.IdPlanEstudios && p.Status == StatusEnum.Active, ct);

            if (!planExiste)
                throw new InvalidOperationException("Plan de estudios no encontrado");

            var periodoExiste = await _dbContext.PeriodoAcademico
                .AnyAsync(p => p.IdPeriodoAcademico == request.IdPeriodoAcademicoDestino && p.Status == StatusEnum.Active, ct);

            if (!periodoExiste)
                throw new InvalidOperationException("Periodo académico destino no encontrado");

            var duplicada = await _dbContext.PreInscripcion
                .AnyAsync(p => p.IdEstudiante == request.IdEstudiante
                    && p.IdPeriodoAcademicoDestino == request.IdPeriodoAcademicoDestino
                    && p.Estado == "Pendiente"
                    && p.Status == StatusEnum.Active, ct);

            if (duplicada)
                throw new InvalidOperationException("El estudiante ya tiene una preinscripción pendiente para este periodo");

            if (!estudiante.Activo
                || estudiante.EstatusAcademico == EstudianteStatusAcademicoEnum.BajaTemporal
                || estudiante.EstatusAcademico == EstudianteStatusAcademicoEnum.BajaDefinitiva)
            {
                estudiante.Activo = true;
                estudiante.EstatusAcademico = EstudianteStatusAcademicoEnum.Cursando;
                estudiante.TipoBaja = null;
                estudiante.EstadoBaja = null;
                estudiante.MotivoBaja = null;
                estudiante.FechaBaja = null;
                estudiante.UpdatedAt = DateTime.UtcNow;
            }

            var preInscripcion = new PreInscripcion
            {
                IdEstudiante = request.IdEstudiante,
                IdPlanEstudios = request.IdPlanEstudios,
                IdPeriodoAcademicoDestino = request.IdPeriodoAcademicoDestino,
                NumeroCuatrimestreObjetivo = request.NumeroCuatrimestreObjetivo,
                Estado = "Pendiente",
                Nota = request.Nota,
                FechaApartado = DateTime.UtcNow,
                Status = StatusEnum.Active,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.PreInscripcion.Add(preInscripcion);
            await _dbContext.SaveChangesAsync(ct);

            return await MapToDtoAsync(preInscripcion.IdPreInscripcion, ct);
        }

        public async Task<List<PreInscripcionDto>> GetPendientesAsync(int? idPlanEstudios = null, int? idPeriodoAcademico = null, CancellationToken ct = default)
        {
            var query = _dbContext.PreInscripcion
                .AsNoTracking()
                .Where(p => p.Estado == "Pendiente" && p.Status == StatusEnum.Active);

            if (idPlanEstudios.HasValue)
                query = query.Where(p => p.IdPlanEstudios == idPlanEstudios.Value);

            if (idPeriodoAcademico.HasValue)
                query = query.Where(p => p.IdPeriodoAcademicoDestino == idPeriodoAcademico.Value);

            return await query
                .Include(p => p.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(p => p.IdPlanEstudiosNavigation)
                .Include(p => p.IdPeriodoAcademicoDestinoNavigation)
                .OrderByDescending(p => p.FechaApartado)
                .Select(p => MapEntityToDto(p))
                .ToListAsync(ct);
        }

        public async Task<PreInscripcionDto> AsignarGrupoAsync(int idPreInscripcion, int idGrupo, CancellationToken ct = default)
        {
            var preInscripcion = await _dbContext.PreInscripcion
                .FirstOrDefaultAsync(p => p.IdPreInscripcion == idPreInscripcion && p.Status == StatusEnum.Active, ct);

            if (preInscripcion == null)
                throw new InvalidOperationException("Preinscripción no encontrada");

            if (preInscripcion.Estado != "Pendiente")
                throw new InvalidOperationException("La preinscripción no está en estado Pendiente");

            var resultado = await _grupoService.InscribirEstudianteAGrupoDirectoAsync(
                idGrupo, preInscripcion.IdEstudiante, preInscripcion.Nota, ct);

            if (!resultado.Exitoso)
                throw new InvalidOperationException(resultado.MensajeError ?? "No se pudo asignar el grupo");

            preInscripcion.Estado = "Inscrito";
            preInscripcion.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            return await MapToDtoAsync(preInscripcion.IdPreInscripcion, ct);
        }

        public async Task<PreInscripcionDto> CancelarAsync(int idPreInscripcion, CancellationToken ct = default)
        {
            var preInscripcion = await _dbContext.PreInscripcion
                .FirstOrDefaultAsync(p => p.IdPreInscripcion == idPreInscripcion && p.Status == StatusEnum.Active, ct);

            if (preInscripcion == null)
                throw new InvalidOperationException("Preinscripción no encontrada");

            preInscripcion.Estado = "Cancelado";
            preInscripcion.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            return await MapToDtoAsync(preInscripcion.IdPreInscripcion, ct);
        }

        private async Task<PreInscripcionDto> MapToDtoAsync(int idPreInscripcion, CancellationToken ct)
        {
            var entity = await _dbContext.PreInscripcion
                .AsNoTracking()
                .Include(p => p.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Include(p => p.IdPlanEstudiosNavigation)
                .Include(p => p.IdPeriodoAcademicoDestinoNavigation)
                .FirstAsync(p => p.IdPreInscripcion == idPreInscripcion, ct);

            return MapEntityToDto(entity);
        }

        private static PreInscripcionDto MapEntityToDto(PreInscripcion p)
        {
            var persona = p.IdEstudianteNavigation?.IdPersonaNavigation;
            var nombreCompleto = persona == null
                ? null
                : $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim();

            return new PreInscripcionDto
            {
                IdPreInscripcion = p.IdPreInscripcion,
                IdEstudiante = p.IdEstudiante,
                Matricula = p.IdEstudianteNavigation?.Matricula,
                NombreCompleto = nombreCompleto,
                IdPlanEstudios = p.IdPlanEstudios,
                ClavePlan = p.IdPlanEstudiosNavigation?.ClavePlanEstudios,
                PlanEstudios = p.IdPlanEstudiosNavigation?.NombrePlanEstudios,
                IdPeriodoAcademicoDestino = p.IdPeriodoAcademicoDestino,
                PeriodoClave = p.IdPeriodoAcademicoDestinoNavigation?.Clave,
                PeriodoNombre = p.IdPeriodoAcademicoDestinoNavigation?.Nombre,
                NumeroCuatrimestreObjetivo = p.NumeroCuatrimestreObjetivo,
                Estado = p.Estado,
                Nota = p.Nota,
                FechaApartado = p.FechaApartado
            };
        }
    }
}
