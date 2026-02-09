using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.Common;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class CalificacionesService : ICalificacionesService
    {
        private readonly ApplicationDbContext _dbContext;

        public CalificacionesService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CalificacionParcial> AbrirParcial(CalificacionParcial newActa)
        {
            await _dbContext.CalificacionesParciales.AddAsync(newActa);
            await _dbContext.SaveChangesAsync();

            return newActa;
        }

        public async Task CambiarEstadoParcial(int calificacionParcialId, string nuevoEstado, string usuario)
        {
            var parcial = await _dbContext.CalificacionesParciales
                .FirstOrDefaultAsync(cp => cp.Id == calificacionParcialId && cp.Status == StatusEnum.Active);

            if (parcial == null)
                throw new InvalidOperationException($"No se encontró el parcial con ID {calificacionParcialId}");

            if (!Enum.TryParse<StatusParcialEnum>(nuevoEstado, true, out var estadoEnum))
                throw new ArgumentException($"Estado inválido: {nuevoEstado}");

            if (estadoEnum == StatusParcialEnum.Cerrado && parcial.StatusParcial != StatusParcialEnum.Cerrado)
            {
                parcial.FechaCierre = DateTime.UtcNow;
            }

            parcial.StatusParcial = estadoEnum;
            parcial.UpdatedAt = DateTime.UtcNow;
            parcial.UpdatedBy = usuario;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<(IList<CalificacionDetalle>, decimal CalificacionFinal)> GetConcentradoAlumno(int inscripcionId)
        {
            var inscripcion = await _dbContext.Inscripcion
                .Include(i => i.IdGrupoMateriaNavigation)
                .FirstOrDefaultAsync(i => i.IdInscripcion == inscripcionId && i.Status == StatusEnum.Active);

            if (inscripcion == null)
                throw new InvalidOperationException($"No se encontró la inscripción con ID {inscripcionId}");

            var detalles = await _dbContext.CalificacionDetalle
                .Include(cd => cd.CalificacionParcial)
                    .ThenInclude(cp => cp.Parcial)
                .Where(cd => cd.CalificacionParcial.InscripcionId == inscripcionId
                          && cd.Status == StatusEnum.Active)
                .OrderBy(cd => cd.CalificacionParcial.ParcialId)
                    .ThenBy(cd => cd.FechaAplicacion)
                .ToListAsync();

            decimal calificacionFinal = 0;
            if (detalles.Any())
            {
                calificacionFinal = detalles.Sum(cd => (cd.Puntos / cd.MaxPuntos) * cd.PesoEvaluacion);
            }

            return (detalles, calificacionFinal);
        }

        public async Task<IList<(int InscripcionId, decimal AporteParcial)>> GetConcentradoGrupoParcial(int grupoMateriaId, int parcialId)
        {
            var parcialesGrupo = await _dbContext.CalificacionesParciales
                .Include(cp => cp.Inscripcion)
                    .ThenInclude(i => i.IdEstudianteNavigation)
                        .ThenInclude(e => e.IdPersonaNavigation)
                .Where(cp => cp.GrupoMateriaId == grupoMateriaId
                          && cp.ParcialId == parcialId
                          && cp.Status == StatusEnum.Active)
                .ToListAsync();

            var resultado = new List<(int InscripcionId, decimal AporteParcial)>();

            foreach (var parcial in parcialesGrupo)
            {
                var detalles = await _dbContext.CalificacionDetalle
                    .Where(cd => cd.CalificacionParcialId == parcial.Id
                              && cd.Status == StatusEnum.Active)
                    .ToListAsync();

                decimal aporteParcial = 0;
                if (detalles.Any())
                {
                    aporteParcial = detalles.Sum(cd => (cd.Puntos / cd.MaxPuntos) * cd.PesoEvaluacion);
                }

                resultado.Add((parcial.InscripcionId, aporteParcial));
            }

            return resultado.OrderByDescending(r => r.AporteParcial).ToList();
        }

        public async Task<PagedResult<CalificacionDetalle>> GetDetalles(int grupoMateriaId, int parcialId, int inscripcionId, int tipoEvaluacionEnum, int page, int pageSize)
        {
            var query = _dbContext.CalificacionDetalle
                .Include(cd => cd.CalificacionParcial)
                    .ThenInclude(cp => cp.Inscripcion)
                        .ThenInclude(i => i.IdEstudianteNavigation)
                            .ThenInclude(e => e.IdPersonaNavigation)
                .Where(cd => cd.Status == StatusEnum.Active);

            if (grupoMateriaId > 0)
                query = query.Where(cd => cd.GrupoMateriaId == grupoMateriaId);

            if (parcialId > 0)
                query = query.Where(cd => cd.CalificacionParcial.ParcialId == parcialId);

            if (inscripcionId > 0)
                query = query.Where(cd => cd.CalificacionParcial.InscripcionId == inscripcionId);

            if (tipoEvaluacionEnum >= 0)
                query = query.Where(cd => (int)cd.TipoEvaluacionEnum == tipoEvaluacionEnum);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(cd => cd.CalificacionParcial.InscripcionId)
                    .ThenBy(cd => cd.FechaAplicacion)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CalificacionDetalle>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<CalificacionParcial> GetParcialById(int id)
        {
            var parcial = await _dbContext.CalificacionesParciales
                .Include(cp => cp.GrupoMateria)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(cp => cp.GrupoMateria)
                    .ThenInclude(gm => gm.IdProfesorNavigation)
                        .ThenInclude(p => p.IdPersonaNavigation)
                .Include(cp => cp.Parcial)
                .Include(cp => cp.Inscripcion)
                    .ThenInclude(i => i.IdEstudianteNavigation)
                        .ThenInclude(e => e.IdPersonaNavigation)
                .Include(cp => cp.Profesor)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .FirstOrDefaultAsync(cp => cp.Id == id && cp.Status == StatusEnum.Active);

            return parcial;
        }

        public async Task<IEnumerable<CalificacionParcial>> GetParcialesPorGrupo(int grupoMateriaId, int parcialId)
        {
            var parciales = await _dbContext.CalificacionesParciales
                .Include(cp => cp.GrupoMateria)
                    .ThenInclude(gm => gm.IdMateriaPlanNavigation)
                        .ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(cp => cp.Parcial)
                .Include(cp => cp.Inscripcion)
                    .ThenInclude(i => i.IdEstudianteNavigation)
                        .ThenInclude(e => e.IdPersonaNavigation)
                .Include(cp => cp.Profesor)
                    .ThenInclude(p => p.IdPersonaNavigation)
                .Where(cp => cp.GrupoMateriaId == grupoMateriaId
                          && cp.ParcialId == parcialId
                          && cp.Status == StatusEnum.Active)
                .OrderBy(cp => cp.Inscripcion.IdEstudianteNavigation.IdPersonaNavigation.Nombre)
                .ToListAsync();

            return parciales;
        }

        public async Task UpsertDetalle(CalificacionDetalle detalle, string applicationUserName)
        {
            var parcial = await _dbContext.CalificacionesParciales
                .FirstOrDefaultAsync(cp => cp.Id == detalle.CalificacionParcialId && cp.Status == StatusEnum.Active);

            if (parcial == null)
                throw new InvalidOperationException($"No se encontró el parcial con ID {detalle.CalificacionParcialId}");

            if (parcial.StatusParcial == StatusParcialEnum.Cerrado)
                throw new InvalidOperationException("No se pueden capturar calificaciones. El parcial está cerrado.");

            if (detalle.Puntos > detalle.MaxPuntos)
                throw new ArgumentException($"Los puntos obtenidos ({detalle.Puntos}) no pueden ser mayores a los puntos máximos ({detalle.MaxPuntos})");

            if (detalle.PesoEvaluacion < 0 || detalle.PesoEvaluacion > 100)
                throw new ArgumentException($"El peso de evaluación debe estar entre 0 y 100. Valor recibido: {detalle.PesoEvaluacion}");

            var detalleExistente = await _dbContext.CalificacionDetalle
                .FirstOrDefaultAsync(cd => cd.CalificacionParcialId == detalle.CalificacionParcialId
                                        && cd.Nombre == detalle.Nombre
                                        && cd.Status == StatusEnum.Active);

            if (detalleExistente != null)
            {
                detalleExistente.TipoEvaluacionEnum = detalle.TipoEvaluacionEnum;
                detalleExistente.PesoEvaluacion = detalle.PesoEvaluacion;
                detalleExistente.MaxPuntos = detalle.MaxPuntos;
                detalleExistente.Puntos = detalle.Puntos;
                detalleExistente.FechaAplicacion = detalle.FechaAplicacion;
                detalleExistente.FechaCaptura = DateTime.UtcNow;
                detalleExistente.ApplicationUserName = applicationUserName;
                detalleExistente.UpdatedAt = DateTime.UtcNow;
                detalleExistente.UpdatedBy = applicationUserName;
            }
            else
            {
                detalle.FechaCaptura = DateTime.UtcNow;
                detalle.ApplicationUserName = applicationUserName;
                detalle.CreatedAt = DateTime.UtcNow;
                detalle.CreatedBy = applicationUserName;
                detalle.Status = StatusEnum.Active;

                await _dbContext.CalificacionDetalle.AddAsync(detalle);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<(bool EsValido, decimal SumaPesos, string Mensaje)> ValidarPesosEvaluacion(int calificacionParcialId)
        {
            var evaluaciones = await _dbContext.CalificacionDetalle
                .Where(cd => cd.CalificacionParcialId == calificacionParcialId
                          && cd.Status == StatusEnum.Active)
                .ToListAsync();

            if (!evaluaciones.Any())
            {
                return (true, 0, "No hay evaluaciones registradas");
            }

            var sumaPesos = evaluaciones.Sum(e => e.PesoEvaluacion);

            if (sumaPesos == 100)
            {
                return (true, sumaPesos, "La suma de pesos es correcta (100%)");
            }
            else if (sumaPesos < 100)
            {
                return (false, sumaPesos, $"Advertencia: La suma de pesos es {sumaPesos}%. Faltan {100 - sumaPesos}% por asignar.");
            }
            else
            {
                return (false, sumaPesos, $"Error: La suma de pesos es {sumaPesos}%. Excede el 100% en {sumaPesos - 100}%.");
            }
        }

        public async Task<bool> ValidarEstadoParcialParaCaptura(int calificacionParcialId)
        {
            var parcial = await _dbContext.CalificacionesParciales
                .FirstOrDefaultAsync(cp => cp.Id == calificacionParcialId && cp.Status == StatusEnum.Active);

            if (parcial == null)
                return false;

            return parcial.StatusParcial == StatusParcialEnum.Abierto;
        }
    }
}
