using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.Titulacion;
using WebApplication2.Core.Enums;
using WebApplication2.Core.Models.Titulacion;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class CertificadoElectronicoService : ICertificadoElectronicoService
    {
        private readonly ApplicationDbContext _db;

        public CertificadoElectronicoService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<CertificadoElectronicoListDto>> ListarAsync(TipoTitulacionEnum tipo, CancellationToken ct = default)
        {
            return await _db.CertificadoElectronico
                .Where(c => c.TipoTitulacion == tipo && c.Status == StatusEnum.Active)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CertificadoElectronicoListDto
                {
                    Id = c.Id,
                    FolioControl = c.FolioControl,
                    TipoTitulacion = (int)c.TipoTitulacion,
                    Estatus = (int)c.Estatus,
                    EstatusTexto = c.Estatus.ToString(),
                    NumeroControl = c.NumeroControl,
                    Curp = c.Curp,
                    NombreCompleto = c.Nombre + " " + c.PrimerApellido + " " + (c.SegundoApellido ?? ""),
                    NombreCarrera = c.NombreCarrera,
                    ClavePlan = c.ClavePlan,
                    Promedio = c.Promedio,
                    TotalAsignaturas = c.TotalAsignaturas,
                    AsignaturasAsignadas = c.AsignaturasAsignadas,
                    FolioControlSEP = c.FolioControlSEP,
                    FechaExpedicion = c.FechaExpedicion,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(ct);
        }

        public async Task<CertificadoElectronicoDetalleDto?> ObtenerDetalleAsync(int id, CancellationToken ct = default)
        {
            var cert = await _db.CertificadoElectronico
                .Include(c => c.Asignaturas.Where(a => a.Status == StatusEnum.Active))
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (cert == null) return null;

            return MapDetalle(cert);
        }

        public async Task<CertificadoElectronicoDetalleDto> CrearDirectoAsync(CrearCertificadoDirectoRequest request, CancellationToken ct = default)
        {
            var cert = new CertificadoElectronico
            {
                TipoTitulacion = TipoTitulacionEnum.Directa,
                Estatus = EstatusCertificadoEnum.Registro,
                NumeroControl = request.NumeroControl,
                Curp = request.Curp?.ToUpper(),
                Nombre = request.Nombre,
                PrimerApellido = request.PrimerApellido,
                SegundoApellido = request.SegundoApellido,
                IdGenero = request.IdGenero,
                FechaNacimiento = DateTime.Parse(request.FechaNacimiento),
                IdCarreraSEP = request.IdCarreraSEP,
                ClaveCarrera = request.ClaveCarrera,
                NombreCarrera = request.NombreCarrera,
                IdTipoPeriodo = request.IdTipoPeriodo,
                TipoPeriodo = request.TipoPeriodo,
                ClavePlan = request.ClavePlan,
                NumeroRvoe = request.NumeroRvoe,
                FechaExpedicionRvoe = DateTime.Parse(request.FechaExpedicionRvoe),
                IdTipoCertificacion = request.IdTipoCertificacion,
                TipoCertificacion = request.TipoCertificacion,
                FechaExpedicion = DateTime.Parse(request.FechaExpedicion),
                IdLugarExpedicion = request.IdLugarExpedicion,
                LugarExpedicion = request.LugarExpedicion,
                IdConfiguracionIPES = request.IdConfiguracionIPES,
                CreatedAt = DateTime.UtcNow,
                Status = StatusEnum.Active
            };

            var asignaturas = request.Asignaturas.Select((a, idx) => new CertificadoAsignatura
            {
                IdAsignatura = a.IdAsignatura > 0 ? a.IdAsignatura : idx + 1,
                ClaveAsignatura = a.ClaveAsignatura,
                Nombre = a.Nombre,
                Ciclo = a.Ciclo,
                Calificacion = a.Calificacion,
                Creditos = a.Creditos,
                IdTipoAsignatura = a.IdTipoAsignatura,
                TipoAsignatura = a.TipoAsignatura,
                IdObservaciones = a.IdObservaciones,
                Observaciones = a.Observaciones,
                CreatedAt = DateTime.UtcNow,
                Status = StatusEnum.Active
            }).ToList();

            cert.TotalAsignaturas = asignaturas.Count;
            cert.AsignaturasAsignadas = asignaturas.Count;
            cert.CreditosObtenidos = asignaturas.Sum(a => a.Creditos ?? 0);
            cert.TotalCreditos = cert.CreditosObtenidos;
            cert.NumeroCiclos = asignaturas.Select(a => a.Ciclo).Distinct().Count();

            if (asignaturas.Count > 0)
            {
                var califs = asignaturas
                    .Select(a => decimal.TryParse(a.Calificacion, out var v) ? v : (decimal?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .ToList();
                if (califs.Count > 0)
                    cert.Promedio = Math.Round(califs.Average(), 2).ToString("F2");
            }

            cert.Asignaturas = asignaturas;
            _db.CertificadoElectronico.Add(cert);
            await _db.SaveChangesAsync(ct);

            return MapDetalle(cert);
        }

        public async Task<CertificadoElectronicoDetalleDto> ActualizarDirectoAsync(int id, CrearCertificadoDirectoRequest request, CancellationToken ct = default)
        {
            var cert = await _db.CertificadoElectronico
                .Include(c => c.Asignaturas)
                .FirstOrDefaultAsync(c => c.Id == id, ct)
                ?? throw new InvalidOperationException("Certificado no encontrado");

            if (cert.Estatus != EstatusCertificadoEnum.Registro && cert.Estatus != EstatusCertificadoEnum.Rechazado)
                throw new InvalidOperationException("Solo se pueden editar certificados en estatus Registro o Rechazado");

            cert.NumeroControl = request.NumeroControl;
            cert.Curp = request.Curp?.ToUpper();
            cert.Nombre = request.Nombre;
            cert.PrimerApellido = request.PrimerApellido;
            cert.SegundoApellido = request.SegundoApellido;
            cert.IdGenero = request.IdGenero;
            cert.FechaNacimiento = DateTime.Parse(request.FechaNacimiento);
            cert.IdCarreraSEP = request.IdCarreraSEP;
            cert.ClaveCarrera = request.ClaveCarrera;
            cert.NombreCarrera = request.NombreCarrera;
            cert.IdTipoPeriodo = request.IdTipoPeriodo;
            cert.TipoPeriodo = request.TipoPeriodo;
            cert.ClavePlan = request.ClavePlan;
            cert.NumeroRvoe = request.NumeroRvoe;
            cert.FechaExpedicionRvoe = DateTime.Parse(request.FechaExpedicionRvoe);
            cert.IdTipoCertificacion = request.IdTipoCertificacion;
            cert.TipoCertificacion = request.TipoCertificacion;
            cert.FechaExpedicion = DateTime.Parse(request.FechaExpedicion);
            cert.IdLugarExpedicion = request.IdLugarExpedicion;
            cert.LugarExpedicion = request.LugarExpedicion;
            cert.UpdatedAt = DateTime.UtcNow;

            foreach (var a in cert.Asignaturas) a.Status = StatusEnum.Deleted;

            var nuevas = request.Asignaturas.Select((a, idx) => new CertificadoAsignatura
            {
                IdCertificadoElectronico = cert.Id,
                IdAsignatura = a.IdAsignatura > 0 ? a.IdAsignatura : idx + 1,
                ClaveAsignatura = a.ClaveAsignatura,
                Nombre = a.Nombre,
                Ciclo = a.Ciclo,
                Calificacion = a.Calificacion,
                IdObservaciones = a.IdObservaciones,
                Observaciones = a.Observaciones,
                CreatedAt = DateTime.UtcNow,
                Status = StatusEnum.Active
            }).ToList();

            _db.CertificadoAsignatura.AddRange(nuevas);

            cert.TotalAsignaturas = nuevas.Count;
            cert.AsignaturasAsignadas = nuevas.Count;

            var califs = nuevas
                .Select(a => decimal.TryParse(a.Calificacion, out var v) ? v : (decimal?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();
            cert.Promedio = califs.Count > 0 ? Math.Round(califs.Average(), 1).ToString("F1") : null;

            await _db.SaveChangesAsync(ct);
            return MapDetalle(cert);
        }

        public async Task<(int total, int enRevision, int xmlGenerados, int registrados)> ObtenerEstadisticasAsync(TipoTitulacionEnum tipo, CancellationToken ct = default)
        {
            var certs = await _db.CertificadoElectronico
                .Where(c => c.TipoTitulacion == tipo && c.Status == StatusEnum.Active)
                .GroupBy(c => c.Estatus)
                .Select(g => new { Estatus = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var total = certs.Sum(c => c.Count);
            var enRevision = certs.Where(c => c.Estatus == EstatusCertificadoEnum.EnRevision).Sum(c => c.Count);
            var xml = certs.Where(c => c.Estatus == EstatusCertificadoEnum.XMLGenerado).Sum(c => c.Count);
            var reg = certs.Where(c => c.Estatus == EstatusCertificadoEnum.Registrado).Sum(c => c.Count);

            return (total, enRevision, xml, reg);
        }

        private static CertificadoElectronicoDetalleDto MapDetalle(CertificadoElectronico c) => new()
        {
            Id = c.Id,
            FolioControl = c.FolioControl,
            TipoTitulacion = (int)c.TipoTitulacion,
            Estatus = (int)c.Estatus,
            EstatusTexto = c.Estatus.ToString(),
            IdEstudiante = c.IdEstudiante,
            IdPersona = c.IdPersona,
            NumeroControl = c.NumeroControl,
            Curp = c.Curp,
            Nombre = c.Nombre,
            PrimerApellido = c.PrimerApellido,
            SegundoApellido = c.SegundoApellido,
            IdGenero = c.IdGenero,
            FechaNacimiento = c.FechaNacimiento,
            IdCarreraSEP = c.IdCarreraSEP,
            ClaveCarrera = c.ClaveCarrera,
            NombreCarrera = c.NombreCarrera,
            IdTipoPeriodo = c.IdTipoPeriodo,
            TipoPeriodo = c.TipoPeriodo,
            ClavePlan = c.ClavePlan,
            NumeroRvoe = c.NumeroRvoe,
            FechaExpedicionRvoe = c.FechaExpedicionRvoe,
            IdTipoCertificacion = c.IdTipoCertificacion,
            TipoCertificacion = c.TipoCertificacion,
            FechaExpedicion = c.FechaExpedicion,
            IdLugarExpedicion = c.IdLugarExpedicion,
            LugarExpedicion = c.LugarExpedicion,
            TotalAsignaturas = c.TotalAsignaturas,
            AsignaturasAsignadas = c.AsignaturasAsignadas,
            Promedio = c.Promedio,
            IdConfiguracionIPES = c.IdConfiguracionIPES,
            XmlGenerado = c.XmlGenerado,
            CadenaOriginal = c.CadenaOriginal,
            SelloDigital = c.SelloDigital,
            NumeroLoteSEP = c.NumeroLoteSEP,
            FolioControlSEP = c.FolioControlSEP,
            MensajeSEP = c.MensajeSEP,
            FechaEnvioSEP = c.FechaEnvioSEP,
            FechaRespuestaSEP = c.FechaRespuestaSEP,
            Asignaturas = c.Asignaturas
                .Where(a => a.Status == StatusEnum.Active)
                .Select(a => new CertificadoAsignaturaDto
                {
                    Id = a.Id,
                    IdAsignatura = a.IdAsignatura,
                    ClaveAsignatura = a.ClaveAsignatura,
                    Nombre = a.Nombre,
                    Ciclo = a.Ciclo,
                    Calificacion = a.Calificacion,
                    IdObservaciones = a.IdObservaciones,
                    Observaciones = a.Observaciones
                })
                .OrderBy(a => a.IdAsignatura)
                .ToList()
        };
    }
}
