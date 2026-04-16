using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.Titulacion;
using WebApplication2.Core.Models.Titulacion;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class ConfiguracionTitulacionService : IConfiguracionTitulacionService
    {
        private readonly ApplicationDbContext _db;

        public ConfiguracionTitulacionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ConfiguracionIPESDto>> GetConfiguracionesAsync(CancellationToken ct = default)
        {
            var configs = await _db.ConfiguracionIPES
                .Include(c => c.CampusNavigation)
                .Where(c => c.Status == Core.Enums.StatusEnum.Active)
                .ToListAsync(ct);

            var result = new List<ConfiguracionIPESDto>();
            foreach (var c in configs)
            {
                var dto = MapConfiguracion(c);
                dto.ResponsableActivo = await GetResponsableActivo(c.Id, ct);
                dto.CredencialActiva = await GetCredencialActiva(c.Id, ct);
                result.Add(dto);
            }
            return result;
        }

        public async Task<ConfiguracionIPESDto?> GetConfiguracionPorCampusAsync(int idCampus, CancellationToken ct = default)
        {
            var config = await _db.ConfiguracionIPES
                .Include(c => c.CampusNavigation)
                .FirstOrDefaultAsync(c => c.IdCampus == idCampus && c.Status == Core.Enums.StatusEnum.Active, ct);

            if (config == null) return null;

            var dto = MapConfiguracion(config);
            dto.ResponsableActivo = await GetResponsableActivo(config.Id, ct);
            dto.CredencialActiva = await GetCredencialActiva(config.Id, ct);
            return dto;
        }

        public async Task<ConfiguracionIPESDto> GuardarConfiguracionAsync(GuardarConfiguracionIPESRequest request, CancellationToken ct = default)
        {
            ConfiguracionIPES config;

            if (request.Id.HasValue && request.Id > 0)
            {
                config = await _db.ConfiguracionIPES.FindAsync(new object[] { request.Id.Value }, ct)
                    ?? throw new InvalidOperationException("Configuración no encontrada");

                config.IdNombreInstitucion = request.IdNombreInstitucion;
                config.NombreInstitucion = request.NombreInstitucion;
                config.IdCampusSEP = request.IdCampusSEP;
                config.CampusSEP = request.CampusSEP;
                config.IdEntidadFederativa = request.IdEntidadFederativa;
                config.EntidadFederativa = request.EntidadFederativa;
                config.IdCampus = request.IdCampus;
                config.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                config = new ConfiguracionIPES
                {
                    IdNombreInstitucion = request.IdNombreInstitucion,
                    NombreInstitucion = request.NombreInstitucion,
                    IdCampusSEP = request.IdCampusSEP,
                    CampusSEP = request.CampusSEP,
                    IdEntidadFederativa = request.IdEntidadFederativa,
                    EntidadFederativa = request.EntidadFederativa,
                    IdCampus = request.IdCampus,
                    Activa = true,
                    CreatedAt = DateTime.UtcNow,
                    Status = Core.Enums.StatusEnum.Active
                };
                _db.ConfiguracionIPES.Add(config);
            }

            await _db.SaveChangesAsync(ct);

            await _db.Entry(config).Reference(c => c.CampusNavigation).LoadAsync(ct);
            return MapConfiguracion(config);
        }

        public async Task<ResponsableFirmaDto> GuardarResponsableAsync(GuardarResponsableFirmaRequest request, CancellationToken ct = default)
        {
            ResponsableFirma responsable;

            if (request.Id.HasValue && request.Id > 0)
            {
                responsable = await _db.ResponsableFirma.FindAsync(new object[] { request.Id.Value }, ct)
                    ?? throw new InvalidOperationException("Responsable no encontrado");

                responsable.Curp = request.Curp.ToUpper();
                responsable.Nombre = request.Nombre;
                responsable.PrimerApellido = request.PrimerApellido;
                responsable.SegundoApellido = request.SegundoApellido;
                responsable.IdCargo = request.IdCargo;
                responsable.Cargo = request.Cargo;
                if (!string.IsNullOrWhiteSpace(request.PasswordLlavePrivada))
                    responsable.PasswordLlavePrivada = request.PasswordLlavePrivada;
                responsable.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var existentes = await _db.ResponsableFirma
                    .Where(r => r.IdConfiguracionIPES == request.IdConfiguracionIPES && r.Activo)
                    .ToListAsync(ct);
                foreach (var e in existentes) e.Activo = false;

                responsable = new ResponsableFirma
                {
                    Curp = request.Curp.ToUpper(),
                    Nombre = request.Nombre,
                    PrimerApellido = request.PrimerApellido,
                    SegundoApellido = request.SegundoApellido,
                    IdCargo = request.IdCargo,
                    Cargo = request.Cargo,
                    PasswordLlavePrivada = request.PasswordLlavePrivada,
                    IdConfiguracionIPES = request.IdConfiguracionIPES,
                    Activo = true,
                    VigenciaInicio = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Status = Core.Enums.StatusEnum.Active
                };
                _db.ResponsableFirma.Add(responsable);
            }

            await _db.SaveChangesAsync(ct);
            return MapResponsable(responsable);
        }

        public async Task<CredencialSEPDto> GuardarCredencialAsync(GuardarCredencialSEPRequest request, CancellationToken ct = default)
        {
            CredencialSEP credencial;

            if (request.Id.HasValue && request.Id > 0)
            {
                credencial = await _db.CredencialSEP.FindAsync(new object[] { request.Id.Value }, ct)
                    ?? throw new InvalidOperationException("Credencial no encontrada");

                credencial.Usuario = request.Usuario;
                if (!string.IsNullOrWhiteSpace(request.Password))
                    credencial.Password = request.Password;
                credencial.EndpointUrl = request.EndpointUrl;
                credencial.EsProduccion = request.EsProduccion;
                credencial.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var existentes = await _db.CredencialSEP
                    .Where(c => c.IdConfiguracionIPES == request.IdConfiguracionIPES && c.Activa)
                    .ToListAsync(ct);
                foreach (var e in existentes) e.Activa = false;

                credencial = new CredencialSEP
                {
                    Usuario = request.Usuario,
                    Password = request.Password ?? "",
                    EndpointUrl = request.EndpointUrl,
                    EsProduccion = request.EsProduccion,
                    IdConfiguracionIPES = request.IdConfiguracionIPES,
                    Activa = true,
                    CreatedAt = DateTime.UtcNow,
                    Status = Core.Enums.StatusEnum.Active
                };
                _db.CredencialSEP.Add(credencial);
            }

            await _db.SaveChangesAsync(ct);
            return MapCredencial(credencial);
        }

        public async Task SubirCertificadoCerAsync(int idResponsable, Stream archivo, string nombreArchivo, CancellationToken ct = default)
        {
            var responsable = await _db.ResponsableFirma.FindAsync(new object[] { idResponsable }, ct)
                ?? throw new InvalidOperationException("Responsable no encontrado");

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "titulacion", "llaves");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"cer_{idResponsable}_{DateTime.UtcNow:yyyyMMddHHmmss}.cer";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(fs, ct);
            }

            responsable.RutaCertificadoCer = filePath;
            responsable.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        public async Task SubirLlaveKeyAsync(int idResponsable, Stream archivo, string nombreArchivo, CancellationToken ct = default)
        {
            var responsable = await _db.ResponsableFirma.FindAsync(new object[] { idResponsable }, ct)
                ?? throw new InvalidOperationException("Responsable no encontrado");

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "titulacion", "llaves");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"key_{idResponsable}_{DateTime.UtcNow:yyyyMMddHHmmss}.key";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(fs, ct);
            }

            responsable.RutaLlavePrivadaKey = filePath;
            responsable.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        private async Task<ResponsableFirmaDto?> GetResponsableActivo(int idConfig, CancellationToken ct)
        {
            var r = await _db.ResponsableFirma
                .FirstOrDefaultAsync(x => x.IdConfiguracionIPES == idConfig && x.Activo && x.Status == Core.Enums.StatusEnum.Active, ct);
            return r != null ? MapResponsable(r) : null;
        }

        private async Task<CredencialSEPDto?> GetCredencialActiva(int idConfig, CancellationToken ct)
        {
            var c = await _db.CredencialSEP
                .FirstOrDefaultAsync(x => x.IdConfiguracionIPES == idConfig && x.Activa && x.Status == Core.Enums.StatusEnum.Active, ct);
            return c != null ? MapCredencial(c) : null;
        }

        private static ConfiguracionIPESDto MapConfiguracion(ConfiguracionIPES c) => new()
        {
            Id = c.Id,
            IdNombreInstitucion = c.IdNombreInstitucion,
            NombreInstitucion = c.NombreInstitucion,
            IdCampusSEP = c.IdCampusSEP,
            CampusSEP = c.CampusSEP,
            IdEntidadFederativa = c.IdEntidadFederativa,
            EntidadFederativa = c.EntidadFederativa,
            IdCampus = c.IdCampus,
            NombreCampus = c.CampusNavigation?.Nombre,
            Activa = c.Activa
        };

        private static ResponsableFirmaDto MapResponsable(ResponsableFirma r) => new()
        {
            Id = r.Id,
            Curp = r.Curp,
            Nombre = r.Nombre,
            PrimerApellido = r.PrimerApellido,
            SegundoApellido = r.SegundoApellido,
            IdCargo = r.IdCargo,
            Cargo = r.Cargo,
            TieneCertificadoCer = !string.IsNullOrWhiteSpace(r.RutaCertificadoCer),
            TieneLlaveKey = !string.IsNullOrWhiteSpace(r.RutaLlavePrivadaKey),
            NoCertificadoResponsable = r.NoCertificadoResponsable,
            Activo = r.Activo,
            VigenciaInicio = r.VigenciaInicio,
            VigenciaFin = r.VigenciaFin
        };

        private static CredencialSEPDto MapCredencial(CredencialSEP c) => new()
        {
            Id = c.Id,
            Usuario = c.Usuario,
            TienePassword = !string.IsNullOrWhiteSpace(c.Password),
            EndpointUrl = c.EndpointUrl,
            EsProduccion = c.EsProduccion,
            Activa = c.Activa
        };
    }
}
