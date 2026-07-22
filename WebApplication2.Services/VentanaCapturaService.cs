using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.VentanaCaptura;
using WebApplication2.Core.Models;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;
using StatusEnum = WebApplication2.Core.Enums.StatusEnum;

namespace WebApplication2.Services
{
    public class VentanaCapturaService : IVentanaCapturaService
    {
        private readonly ApplicationDbContext _db;

        public VentanaCapturaService(ApplicationDbContext db)
        {
            _db = db;
        }

        private static DateTime? ToUtc(DateTime? d)
        {
            if (d == null) return null;
            return d.Value.Kind switch
            {
                DateTimeKind.Utc => d,
                DateTimeKind.Local => d.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(d.Value, DateTimeKind.Utc),
            };
        }

        private static bool Vigente(VentanaCaptura v) =>
            v.Abierta && (v.FechaLimite == null || DateTime.UtcNow <= v.FechaLimite);

        private static VentanaCapturaDto Map(VentanaCaptura v) => new()
        {
            IdVentanaCaptura = v.IdVentanaCaptura,
            IdPeriodoAcademico = v.IdPeriodoAcademico,
            NumeroParcial = v.NumeroParcial,
            Abierta = v.Abierta,
            FechaApertura = v.FechaApertura,
            FechaLimite = v.FechaLimite,
            Vigente = Vigente(v),
        };

        public async Task<List<VentanaCapturaDto>> GetVentanasAsync(int idPeriodoAcademico, CancellationToken ct = default)
        {
            var ventanas = await _db.VentanasCaptura
                .Where(v => v.IdPeriodoAcademico == idPeriodoAcademico && v.Status == StatusEnum.Active)
                .ToListAsync(ct);

            var result = new List<VentanaCapturaDto>();
            for (int p = 1; p <= 3; p++)
            {
                var v = ventanas.FirstOrDefault(x => x.NumeroParcial == p);
                result.Add(v == null
                    ? new VentanaCapturaDto { IdPeriodoAcademico = idPeriodoAcademico, NumeroParcial = p, Abierta = false, Vigente = false }
                    : Map(v));
            }
            return result;
        }

        public async Task<VentanaCapturaDto> AbrirAsync(int idPeriodoAcademico, int numeroParcial, DateTime? fechaLimite, CancellationToken ct = default)
        {
            var v = await _db.VentanasCaptura
                .FirstOrDefaultAsync(x => x.IdPeriodoAcademico == idPeriodoAcademico && x.NumeroParcial == numeroParcial && x.Status == StatusEnum.Active, ct);

            if (v == null)
            {
                v = new VentanaCaptura
                {
                    IdPeriodoAcademico = idPeriodoAcademico,
                    NumeroParcial = numeroParcial,
                    Status = StatusEnum.Active,
                    CreatedAt = DateTime.UtcNow,
                };
                _db.VentanasCaptura.Add(v);
            }

            v.Abierta = true;
            v.FechaApertura = DateTime.UtcNow;
            v.FechaLimite = ToUtc(fechaLimite);
            v.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return Map(v);
        }

        public async Task<VentanaCapturaDto> CerrarAsync(int idPeriodoAcademico, int numeroParcial, CancellationToken ct = default)
        {
            var v = await _db.VentanasCaptura
                .FirstOrDefaultAsync(x => x.IdPeriodoAcademico == idPeriodoAcademico && x.NumeroParcial == numeroParcial && x.Status == StatusEnum.Active, ct);

            if (v == null)
            {
                v = new VentanaCaptura
                {
                    IdPeriodoAcademico = idPeriodoAcademico,
                    NumeroParcial = numeroParcial,
                    Status = StatusEnum.Active,
                    CreatedAt = DateTime.UtcNow,
                };
                _db.VentanasCaptura.Add(v);
            }

            v.Abierta = false;
            v.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return Map(v);
        }

        public async Task<(bool permitido, string? motivo)> PuedeCapturarAsync(int idGrupoMateria, int numeroParcial, int idProfesor, CancellationToken ct = default)
        {
            var idPeriodo = await _db.GrupoMateria
                .Where(gm => gm.IdGrupoMateria == idGrupoMateria)
                .Select(gm => (int?)gm.IdGrupoNavigation.IdPeriodoAcademico)
                .FirstOrDefaultAsync(ct);

            if (idPeriodo == null) return (true, null);

            var v = await _db.VentanasCaptura
                .FirstOrDefaultAsync(x => x.IdPeriodoAcademico == idPeriodo && x.NumeroParcial == numeroParcial && x.Status == StatusEnum.Active, ct);

            if (v == null) return (true, null);
            if (Vigente(v)) return (true, null);

            var prorroga = await _db.SolicitudesProrrogaCaptura
                .Where(s => s.IdGrupoMateria == idGrupoMateria && s.NumeroParcial == numeroParcial
                    && s.IdProfesor == idProfesor && s.Estado == "Aprobada" && s.Status == StatusEnum.Active)
                .OrderByDescending(s => s.FechaLimiteProrroga)
                .FirstOrDefaultAsync(ct);

            if (prorroga?.FechaLimiteProrroga != null && DateTime.UtcNow <= prorroga.FechaLimiteProrroga)
                return (true, null);

            var motivo = (v.Abierta && v.FechaLimite != null)
                ? $"La captura del Parcial {numeroParcial} venció el {v.FechaLimite:dd/MM/yyyy HH:mm}. Puedes solicitar una prórroga."
                : $"La captura del Parcial {numeroParcial} está cerrada. Puedes solicitar una prórroga.";
            return (false, motivo);
        }

        public async Task<EstadoCapturaDto> GetEstadoAsync(int idGrupoMateria, int numeroParcial, int idProfesor, CancellationToken ct = default)
        {
            var idPeriodo = await _db.GrupoMateria
                .Where(gm => gm.IdGrupoMateria == idGrupoMateria)
                .Select(gm => (int?)gm.IdGrupoNavigation.IdPeriodoAcademico)
                .FirstOrDefaultAsync(ct);

            var v = idPeriodo == null ? null : await _db.VentanasCaptura
                .FirstOrDefaultAsync(x => x.IdPeriodoAcademico == idPeriodo && x.NumeroParcial == numeroParcial && x.Status == StatusEnum.Active, ct);

            var prorroga = await _db.SolicitudesProrrogaCaptura
                .Where(s => s.IdGrupoMateria == idGrupoMateria && s.NumeroParcial == numeroParcial
                    && s.IdProfesor == idProfesor && s.Status == StatusEnum.Active)
                .OrderByDescending(s => s.FechaSolicitud)
                .FirstOrDefaultAsync(ct);

            var (permitido, motivo) = await PuedeCapturarAsync(idGrupoMateria, numeroParcial, idProfesor, ct);

            var prorrogaAprobadaVigente = prorroga?.Estado == "Aprobada"
                && prorroga.FechaLimiteProrroga != null && DateTime.UtcNow <= prorroga.FechaLimiteProrroga;

            return new EstadoCapturaDto
            {
                NumeroParcial = numeroParcial,
                PuedeCapturar = permitido,
                VentanaAbierta = v == null ? true : Vigente(v),
                FechaLimite = v?.FechaLimite,
                TieneProrrogaAprobada = prorrogaAprobadaVigente,
                FechaLimiteProrroga = prorrogaAprobadaVigente ? prorroga!.FechaLimiteProrroga : null,
                ProrrogaPendiente = prorroga?.Estado == "Pendiente" ? "Pendiente" : null,
                Mensaje = permitido
                    ? (v == null ? "Captura disponible." : "Captura abierta.")
                    : (motivo ?? "Captura cerrada."),
            };
        }

        public async Task<SolicitudProrrogaDto> SolicitarProrrogaAsync(int idProfesor, SolicitarProrrogaRequest req, CancellationToken ct = default)
        {
            var yaPendiente = await _db.SolicitudesProrrogaCaptura
                .AnyAsync(s => s.IdProfesor == idProfesor && s.IdGrupoMateria == req.IdGrupoMateria
                    && s.NumeroParcial == req.NumeroParcial && s.Estado == "Pendiente" && s.Status == StatusEnum.Active, ct);

            if (yaPendiente)
                throw new InvalidOperationException("Ya tienes una solicitud de prórroga pendiente para este parcial.");

            var solicitud = new SolicitudProrrogaCaptura
            {
                IdProfesor = idProfesor,
                IdGrupoMateria = req.IdGrupoMateria,
                NumeroParcial = req.NumeroParcial,
                Motivo = req.Motivo,
                FechaSolicitud = DateTime.UtcNow,
                Estado = "Pendiente",
                Status = StatusEnum.Active,
                CreatedAt = DateTime.UtcNow,
            };
            _db.SolicitudesProrrogaCaptura.Add(solicitud);
            await _db.SaveChangesAsync(ct);

            return (await GetProrrogasQuery().FirstAsync(s => s.IdSolicitudProrroga == solicitud.IdSolicitudProrroga, ct));
        }

        private IQueryable<SolicitudProrrogaDto> GetProrrogasQuery() =>
            _db.SolicitudesProrrogaCaptura
                .Where(s => s.Status == StatusEnum.Active)
                .Select(s => new SolicitudProrrogaDto
                {
                    IdSolicitudProrroga = s.IdSolicitudProrroga,
                    IdProfesor = s.IdProfesor,
                    Profesor = (s.IdProfesorNavigation!.IdPersonaNavigation!.Nombre + " " +
                                s.IdProfesorNavigation.IdPersonaNavigation.ApellidoPaterno + " " +
                                s.IdProfesorNavigation.IdPersonaNavigation.ApellidoMaterno).Trim(),
                    IdGrupoMateria = s.IdGrupoMateria,
                    Grupo = s.IdGrupoMateriaNavigation!.IdGrupoNavigation!.CodigoGrupo ?? "",
                    Materia = s.IdGrupoMateriaNavigation.IdMateriaPlanNavigation!.IdMateriaNavigation!.Nombre ?? "",
                    NumeroParcial = s.NumeroParcial,
                    Motivo = s.Motivo,
                    FechaSolicitud = s.FechaSolicitud,
                    Estado = s.Estado,
                    FechaLimiteProrroga = s.FechaLimiteProrroga,
                    FechaResolucion = s.FechaResolucion,
                    NotaResolucion = s.NotaResolucion,
                });

        public async Task<List<SolicitudProrrogaDto>> GetProrrogasAsync(string? estado, int? campusRestringido, CancellationToken ct = default)
        {
            var query = _db.SolicitudesProrrogaCaptura.Where(s => s.Status == StatusEnum.Active);

            if (!string.IsNullOrWhiteSpace(estado))
                query = query.Where(s => s.Estado == estado);

            if (campusRestringido != null)
                query = query.Where(s => s.IdGrupoMateriaNavigation!.IdGrupoNavigation!.IdPlanEstudiosNavigation!.IdCampus == campusRestringido);

            return await query
                .OrderByDescending(s => s.FechaSolicitud)
                .Select(s => new SolicitudProrrogaDto
                {
                    IdSolicitudProrroga = s.IdSolicitudProrroga,
                    IdProfesor = s.IdProfesor,
                    Profesor = (s.IdProfesorNavigation!.IdPersonaNavigation!.Nombre + " " +
                                s.IdProfesorNavigation.IdPersonaNavigation.ApellidoPaterno + " " +
                                s.IdProfesorNavigation.IdPersonaNavigation.ApellidoMaterno).Trim(),
                    IdGrupoMateria = s.IdGrupoMateria,
                    Grupo = s.IdGrupoMateriaNavigation!.IdGrupoNavigation!.CodigoGrupo ?? "",
                    Materia = s.IdGrupoMateriaNavigation.IdMateriaPlanNavigation!.IdMateriaNavigation!.Nombre ?? "",
                    NumeroParcial = s.NumeroParcial,
                    Motivo = s.Motivo,
                    FechaSolicitud = s.FechaSolicitud,
                    Estado = s.Estado,
                    FechaLimiteProrroga = s.FechaLimiteProrroga,
                    FechaResolucion = s.FechaResolucion,
                    NotaResolucion = s.NotaResolucion,
                })
                .ToListAsync(ct);
        }

        public async Task<List<SolicitudProrrogaDto>> GetMisProrrogasAsync(int idProfesor, CancellationToken ct = default)
        {
            return await GetProrrogasQuery()
                .Where(s => s.IdProfesor == idProfesor)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync(ct);
        }

        public async Task<SolicitudProrrogaDto?> ResolverProrrogaAsync(int idSolicitud, ResolverProrrogaRequest req, string userId, CancellationToken ct = default)
        {
            var solicitud = await _db.SolicitudesProrrogaCaptura
                .FirstOrDefaultAsync(s => s.IdSolicitudProrroga == idSolicitud && s.Status == StatusEnum.Active, ct);

            if (solicitud == null) return null;

            solicitud.Estado = req.Aprobar ? "Aprobada" : "Rechazada";
            solicitud.FechaLimiteProrroga = req.Aprobar ? ToUtc(req.FechaLimiteProrroga) : null;
            solicitud.NotaResolucion = req.Nota;
            solicitud.ResueltaPor = userId;
            solicitud.FechaResolucion = DateTime.UtcNow;
            solicitud.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return await GetProrrogasQuery().FirstOrDefaultAsync(s => s.IdSolicitudProrroga == idSolicitud, ct);
        }

        public async Task<AvanceCapturaDto> GetAvanceAsync(int idPeriodoAcademico, int numeroParcial, int? campusRestringido, CancellationToken ct = default)
        {
            var query = _db.GrupoMateria
                .Include(gm => gm.IdMateriaPlanNavigation).ThenInclude(mp => mp.IdMateriaNavigation)
                .Include(gm => gm.IdGrupoNavigation).ThenInclude(g => g.IdPlanEstudiosNavigation).ThenInclude(p => p.IdCampusNavigation)
                .Include(gm => gm.IdProfesorNavigation).ThenInclude(p => p.IdPersonaNavigation)
                .Where(gm => gm.Status == StatusEnum.Active
                    && gm.IdGrupoNavigation.IdPeriodoAcademico == idPeriodoAcademico
                    && gm.IdGrupoNavigation.Status == StatusEnum.Active);

            if (campusRestringido != null)
                query = query.Where(gm => gm.IdGrupoNavigation.IdPlanEstudiosNavigation!.IdCampus == campusRestringido);

            var materias = await query.ToListAsync(ct);
            var idsGm = materias.Select(m => m.IdGrupoMateria).ToList();

            var parcialIds = await _db.CalificacionesParciales
                .Where(cp => cp.Status == StatusEnum.Active && cp.ParcialId == numeroParcial && idsGm.Contains(cp.GrupoMateriaId))
                .Select(cp => cp.Id)
                .ToListAsync(ct);

            var capturas = await _db.CalificacionDetalle
                .Where(d => d.Status == StatusEnum.Active && parcialIds.Contains(d.CalificacionParcialId))
                .GroupBy(d => d.GrupoMateriaId)
                .Select(g => new { Gm = g.Key, Ultima = (DateTime?)g.Max(x => x.FechaCaptura) })
                .ToListAsync(ct);

            var capMap = capturas.ToDictionary(x => x.Gm, x => x.Ultima);

            var items = materias.Select(m =>
            {
                var capturado = capMap.TryGetValue(m.IdGrupoMateria, out var ultima);
                var persona = m.IdProfesorNavigation?.IdPersonaNavigation;
                return new AvanceCapturaItemDto
                {
                    IdGrupoMateria = m.IdGrupoMateria,
                    Grupo = m.IdGrupoNavigation?.CodigoGrupo ?? "",
                    Materia = m.IdMateriaPlanNavigation?.IdMateriaNavigation?.Nombre ?? "",
                    IdProfesor = m.IdProfesor,
                    Profesor = persona != null
                        ? $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim()
                        : "Sin asignar",
                    IdCampus = m.IdGrupoNavigation?.IdPlanEstudiosNavigation?.IdCampus,
                    Campus = m.IdGrupoNavigation?.IdPlanEstudiosNavigation?.IdCampusNavigation?.Nombre,
                    Estado = capturado ? "Capturado" : "Pendiente",
                    UltimaActualizacion = capturado ? ultima : null,
                };
            })
            .OrderBy(i => i.Estado == "Capturado")
            .ThenBy(i => i.Grupo)
            .ToList();

            return new AvanceCapturaDto
            {
                IdPeriodoAcademico = idPeriodoAcademico,
                NumeroParcial = numeroParcial,
                Total = items.Count,
                Capturados = items.Count(i => i.Estado == "Capturado"),
                Pendientes = items.Count(i => i.Estado == "Pendiente"),
                Items = items,
            };
        }
    }
}
