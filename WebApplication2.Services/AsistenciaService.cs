using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Grupo;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Asistencia;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class AsistenciaService : IAsistenciaService
    {
        private readonly ApplicationDbContext _dbContext;

        public AsistenciaService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Asistencia> RegistrarAsistencia(Asistencia asistencia, int profesorId)
        {
            var inscripcion = await _dbContext.Inscripcion
                .Include(i => i.IdGrupoMateriaNavigation)
                .FirstOrDefaultAsync(i => i.IdInscripcion == asistencia.InscripcionId && i.Status == StatusEnum.Active);

            if (inscripcion == null)
                throw new InvalidOperationException($"No se encontró la inscripción con ID {asistencia.InscripcionId}");

            if (inscripcion.IdGrupoMateria != asistencia.GrupoMateriaId)
                throw new InvalidOperationException("El grupo-materia no coincide con la inscripción del estudiante");

            var asistenciaExistente = await _dbContext.Asistencia
                .AnyAsync(a => a.InscripcionId == asistencia.InscripcionId
                            && a.FechaSesion.Date == asistencia.FechaSesion.Date
                            && a.Status == StatusEnum.Active);

            if (asistenciaExistente)
                throw new InvalidOperationException($"Ya existe un registro de asistencia para este estudiante en la fecha {asistencia.FechaSesion:yyyy-MM-dd}");

            asistencia.ProfesorRegistroId = profesorId;
            asistencia.FechaRegistro = DateTime.UtcNow;
            asistencia.CreatedAt = DateTime.UtcNow;
            asistencia.CreatedBy = profesorId.ToString();
            asistencia.Status = StatusEnum.Active;

            await _dbContext.Asistencia.AddAsync(asistencia);
            await _dbContext.SaveChangesAsync();

            return asistencia;
        }

        public async Task<List<Asistencia>> RegistrarAsistenciaMasiva(List<Asistencia> asistencias, int profesorId)
        {
            if (asistencias == null || !asistencias.Any())
                throw new ArgumentException("La lista de asistencias no puede estar vacía");

            var asistenciasCreadas = new List<Asistencia>();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var asistencia in asistencias)
                    {
                        var asistenciaCreada = await RegistrarAsistencia(asistencia, profesorId);
                        asistenciasCreadas.Add(asistenciaCreada);
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return asistenciasCreadas;
        }

        public async Task<Asistencia> ActualizarAsistencia(Asistencia asistenciaActualizada, string usuario)
        {
            var asistencia = await _dbContext.Asistencia
                .FirstOrDefaultAsync(a => a.IdAsistencia == asistenciaActualizada.IdAsistencia && a.Status == StatusEnum.Active);

            if (asistencia == null)
                throw new InvalidOperationException($"No se encontró la asistencia con ID {asistenciaActualizada.IdAsistencia}");

            asistencia.EstadoAsistencia = asistenciaActualizada.EstadoAsistencia;
            asistencia.Observaciones = asistenciaActualizada.Observaciones;
            asistencia.UpdatedAt = DateTime.UtcNow;
            asistencia.UpdatedBy = usuario;

            await _dbContext.SaveChangesAsync();

            return asistencia;
        }

        public async Task<Asistencia> GetAsistenciaById(int idAsistencia)
        {
            var asistencia = await _dbContext.Asistencia
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.IdEstudianteNavigation)
                        .ThenInclude(e => e.IdPersonaNavigation)
                .Include(a => a.GrupoMateria)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(a => a.ProfesorRegistro)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .FirstOrDefaultAsync(a => a.IdAsistencia == idAsistencia && a.Status == StatusEnum.Active);

            return asistencia;
        }

        public async Task<List<Asistencia>> GetAsistenciasPorGrupoMateria(int grupoMateriaId, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var query = _dbContext.Asistencia
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.IdEstudianteNavigation)
                        .ThenInclude(e => e.IdPersonaNavigation)
                .Include(a => a.ProfesorRegistro)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .Where(a => a.GrupoMateriaId == grupoMateriaId && a.Status == StatusEnum.Active);

            if (fechaInicio.HasValue)
                query = query.Where(a => a.FechaSesion >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.FechaSesion <= fechaFin.Value);

            var asistencias = await query
                .OrderByDescending(a => a.FechaSesion)
                    .ThenBy(a => a.Inscripcion.IdEstudianteNavigation.IdPersonaNavigation.Nombre)
                .ToListAsync();

            return asistencias;
        }

        public async Task<List<Asistencia>> GetAsistenciasPorInscripcion(int inscripcionId)
        {
            var asistencias = await _dbContext.Asistencia
                .Include(a => a.GrupoMateria)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(a => a.ProfesorRegistro)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .Where(a => a.InscripcionId == inscripcionId && a.Status == StatusEnum.Active)
                .OrderByDescending(a => a.FechaSesion)
                .ToListAsync();

            return asistencias;
        }

        public async Task<PagedResult<Asistencia>> GetAsistenciasConFiltros(
            int? grupoMateriaId,
            int? inscripcionId,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int? estadoAsistencia,
            int page,
            int pageSize)
        {
            var query = _dbContext.Asistencia
                .Include(a => a.Inscripcion)
                    .ThenInclude(i => i.IdEstudianteNavigation)
                        .ThenInclude(e => e.IdPersonaNavigation)
                .Include(a => a.GrupoMateria)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(a => a.ProfesorRegistro)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .Where(a => a.Status == StatusEnum.Active);

            if (grupoMateriaId.HasValue)
                query = query.Where(a => a.GrupoMateriaId == grupoMateriaId.Value);

            if (inscripcionId.HasValue)
                query = query.Where(a => a.InscripcionId == inscripcionId.Value);

            if (fechaInicio.HasValue)
                query = query.Where(a => a.FechaSesion >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(a => a.FechaSesion <= fechaFin.Value);

            if (estadoAsistencia.HasValue)
                query = query.Where(a => (int)a.EstadoAsistencia == estadoAsistencia.Value);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.FechaSesion)
                    .ThenBy(a => a.Inscripcion.IdEstudianteNavigation.IdPersonaNavigation.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Asistencia>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<(int TotalSesiones, int Presentes, int Ausentes, int Retardos, int Justificadas, decimal PorcentajeAsistencia)> GetEstadisticasAsistencia(int inscripcionId)
        {
            var asistencias = await _dbContext.Asistencia
                .Where(a => a.InscripcionId == inscripcionId && a.Status == StatusEnum.Active)
                .ToListAsync();

            if (!asistencias.Any())
                return (0, 0, 0, 0, 0, 0);

            var totalSesiones = asistencias.Count;
            var presentes = asistencias.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Presente);
            var ausentes = asistencias.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Ausente);
            var retardos = asistencias.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Retardo);
            var justificadas = asistencias.Count(a => a.EstadoAsistencia == EstadoAsistenciaEnum.Justificada);

            var asistenciasValidas = presentes + justificadas;
            var porcentajeAsistencia = totalSesiones > 0 ? (decimal)asistenciasValidas / totalSesiones * 100 : 0;

            return (totalSesiones, presentes, ausentes, retardos, justificadas, Math.Round(porcentajeAsistencia, 2));
        }

        public async Task<List<Asistencia>> RegistrarAsistenciasPorFecha(
            int idGrupoMateria,
            DateTime fecha,
            List<AsistenciaItemRequest> asistencias,
            int profesorId)
        {
            if (asistencias == null || !asistencias.Any())
                throw new ArgumentException("La lista de asistencias no puede estar vacía");

            var asistenciasRegistradas = new List<Asistencia>();

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var asistenciaRequest in asistencias)
                    {
                        var asistenciaExistente = await _dbContext.Asistencia
                            .FirstOrDefaultAsync(a =>
                                a.InscripcionId == asistenciaRequest.IdInscripcion &&
                                a.GrupoMateriaId == idGrupoMateria &&
                                a.FechaSesion.Date == fecha.Date &&
                                a.Status == StatusEnum.Active);

                        if (asistenciaExistente != null)
                        {
                            asistenciaExistente.EstadoAsistencia = asistenciaRequest.Presente
                                ? (asistenciaRequest.Justificada ? EstadoAsistenciaEnum.Justificada : EstadoAsistenciaEnum.Presente)
                                : (asistenciaRequest.Justificada ? EstadoAsistenciaEnum.Justificada : EstadoAsistenciaEnum.Ausente);
                            asistenciaExistente.Observaciones = asistenciaRequest.MotivoJustificacion;
                            asistenciaExistente.UpdatedAt = DateTime.UtcNow;
                            asistenciaExistente.UpdatedBy = profesorId.ToString();

                            asistenciasRegistradas.Add(asistenciaExistente);
                        }
                        else
                        {
                            var nuevaAsistencia = new Asistencia
                            {
                                InscripcionId = asistenciaRequest.IdInscripcion,
                                GrupoMateriaId = idGrupoMateria,
                                FechaSesion = fecha,
                                EstadoAsistencia = asistenciaRequest.Presente
                                    ? (asistenciaRequest.Justificada ? EstadoAsistenciaEnum.Justificada : EstadoAsistenciaEnum.Presente)
                                    : (asistenciaRequest.Justificada ? EstadoAsistenciaEnum.Justificada : EstadoAsistenciaEnum.Ausente),
                                Observaciones = asistenciaRequest.MotivoJustificacion,
                                ProfesorRegistroId = profesorId,
                                FechaRegistro = DateTime.UtcNow,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = profesorId.ToString(),
                                Status = StatusEnum.Active
                            };

                            await _dbContext.Asistencia.AddAsync(nuevaAsistencia);
                            asistenciasRegistradas.Add(nuevaAsistencia);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return asistenciasRegistradas;
        }

        public async Task<List<AsistenciaEstudianteDto>> GetAsistenciasPorGrupoMateriaYFecha(
            int idGrupoMateria,
            DateTime fecha)
        {
            var inscripciones = await _dbContext.Inscripcion
                .Include(i => i.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Where(i => i.IdGrupoMateria == idGrupoMateria && i.Status == StatusEnum.Active)
                .ToListAsync();

            var asistenciasRegistradas = await _dbContext.Asistencia
                .Where(a =>
                    a.GrupoMateriaId == idGrupoMateria &&
                    a.FechaSesion.Date == fecha.Date &&
                    a.Status == StatusEnum.Active)
                .ToListAsync();

            var resultado = inscripciones.Select(inscripcion =>
            {
                var asistencia = asistenciasRegistradas.FirstOrDefault(a => a.InscripcionId == inscripcion.IdInscripcion);
                var persona = inscripcion.IdEstudianteNavigation.IdPersonaNavigation;

                return new AsistenciaEstudianteDto
                {
                    IdAsistencia = asistencia?.IdAsistencia,
                    IdInscripcion = inscripcion.IdInscripcion,
                    IdEstudiante = inscripcion.IdEstudiante,
                    Matricula = inscripcion.IdEstudianteNavigation.Matricula,
                    NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim(),
                    Presente = asistencia != null
                        ? (asistencia.EstadoAsistencia == EstadoAsistenciaEnum.Presente || asistencia.EstadoAsistencia == EstadoAsistenciaEnum.Retardo)
                        : null,
                    Justificada = asistencia?.EstadoAsistencia == EstadoAsistenciaEnum.Justificada,
                    MotivoJustificacion = asistencia?.Observaciones,
                    HoraRegistro = asistencia?.FechaRegistro.ToString("HH:mm:ss")
                };
            })
            .OrderBy(a => a.NombreCompleto)
            .ToList();

            return resultado;
        }

        public async Task<DiasClaseMateriaDto> GetDiasClaseMateria(int idGrupoMateria)
        {
            var grupoMateria = await _dbContext.GrupoMateria
                .Include(gm => gm.IdMateriaPlanNavigation)
                    .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(gm => gm.Horario)
                    .ThenInclude(h => h.IdDiaSemanaNavigation)
                .FirstOrDefaultAsync(gm => gm.IdGrupoMateria == idGrupoMateria && gm.Status == StatusEnum.Active);

            if (grupoMateria == null)
                return null;

            var diasSemana = grupoMateria.Horario
                .Select(h => h.IdDiaSemanaNavigation.Nombre)
                .Distinct()
                .ToList();

            var horarios = grupoMateria.Horario.Select(h => new HorarioClaseDto
            {
                Dia = h.IdDiaSemanaNavigation.Nombre,
                HoraInicio = h.HoraInicio.ToString("HH:mm"),
                HoraFin = h.HoraFin.ToString("HH:mm"),
                Aula = h.Aula ?? grupoMateria.Aula ?? "N/A"
            }).ToList();

            return new DiasClaseMateriaDto
            {
                IdGrupoMateria = idGrupoMateria,
                NombreMateria = grupoMateria.IdMateriaPlanNavigation.IdMateriaNavigation.Nombre,
                DiasSemana = diasSemana,
                Horarios = horarios
            };
        }

        public async Task<List<ResumenAsistenciasDto>> GetResumenAsistenciasPorGrupoMateria(int idGrupoMateria)
        {
            var inscripciones = await _dbContext.Inscripcion
                .Include(i => i.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Where(i => i.IdGrupoMateria == idGrupoMateria && i.Status == StatusEnum.Active)
                .ToListAsync();

            var asistencias = await _dbContext.Asistencia
                .Where(a => a.GrupoMateriaId == idGrupoMateria && a.Status == StatusEnum.Active)
                .ToListAsync();

            var totalClases = asistencias
                .Select(a => a.FechaSesion.Date)
                .Distinct()
                .Count();

            var resumen = inscripciones.Select(inscripcion =>
            {
                var asistenciasEstudiante = asistencias
                    .Where(a => a.InscripcionId == inscripcion.IdInscripcion)
                    .ToList();

                var presentes = asistenciasEstudiante.Count(a =>
                    a.EstadoAsistencia == EstadoAsistenciaEnum.Presente ||
                    a.EstadoAsistencia == EstadoAsistenciaEnum.Retardo);

                var ausentes = asistenciasEstudiante.Count(a =>
                    a.EstadoAsistencia == EstadoAsistenciaEnum.Ausente);

                var faltasJustificadas = asistenciasEstudiante.Count(a =>
                    a.EstadoAsistencia == EstadoAsistenciaEnum.Justificada);

                var faltasInjustificadas = ausentes;

                var porcentajeAsistencia = totalClases > 0
                    ? Math.Round((decimal)presentes / totalClases * 100, 2)
                    : 0;

                var alerta = totalClases > 0 && ((decimal)(ausentes + faltasJustificadas) / totalClases) > 0.20m;

                var persona = inscripcion.IdEstudianteNavigation.IdPersonaNavigation;

                return new ResumenAsistenciasDto
                {
                    IdEstudiante = inscripcion.IdEstudiante,
                    Matricula = inscripcion.IdEstudianteNavigation.Matricula,
                    NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim(),
                    TotalClases = totalClases,
                    Asistencias = presentes,
                    Faltas = ausentes + faltasJustificadas,
                    FaltasJustificadas = faltasJustificadas,
                    FaltasInjustificadas = faltasInjustificadas,
                    PorcentajeAsistencia = porcentajeAsistencia,
                    Alerta = alerta
                };
            })
            .OrderBy(r => r.NombreCompleto)
            .ToList();

            return resumen;
        }
    }
}
