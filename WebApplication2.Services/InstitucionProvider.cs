using WebApplication2.Core.DTOs.Comprobante;
using WebApplication2.Core.DTOs.Recibo;
using WebApplication2.Services.Interfaces;
using WebApplication2.Services.MultiTenant;

namespace WebApplication2.Services
{
    public interface IInstitucionProvider
    {
        InstitucionPdfDto ObtenerPdf();
        InstitucionInfo ObtenerInfo();
        string? ResolverLogoTenant();
    }

    public class InstitucionProvider : IInstitucionProvider
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly IBlobStorageService _blobStorage;

        public InstitucionProvider(ITenantContextAccessor tenantContextAccessor, IBlobStorageService blobStorage)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _blobStorage = blobStorage;
        }

        public string? ResolverLogoTenant()
        {
            var logoUrl = _tenantContextAccessor.TenantContext?.Settings?.LogoUrl;
            if (string.IsNullOrWhiteSpace(logoUrl))
                return null;

            if (!logoUrl.Contains("tenant-logos", System.StringComparison.OrdinalIgnoreCase))
                return null;

            return _blobStorage.ResolverRutaLocal(logoUrl);
        }

        public InstitucionPdfDto ObtenerPdf()
        {
            var def = new InstitucionPdfDto();
            var ctx = _tenantContextAccessor.TenantContext;
            if (ctx == null)
                return def;

            var s = ctx.Settings;
            var configurado = !string.IsNullOrWhiteSpace(s.Direccion) || !string.IsNullOrWhiteSpace(s.RFC);

            return new InstitucionPdfDto
            {
                Nombre = configurado && !string.IsNullOrWhiteSpace(ctx.Nombre) ? ctx.Nombre : def.Nombre,
                Campus = def.Campus,
                Direccion = !string.IsNullOrWhiteSpace(s.Direccion) ? s.Direccion! : def.Direccion,
                Telefono = !string.IsNullOrWhiteSpace(s.Telefono) ? s.Telefono : def.Telefono,
                Email = !string.IsNullOrWhiteSpace(s.Email) ? s.Email : def.Email,
                RFC = !string.IsNullOrWhiteSpace(s.RFC) ? s.RFC : def.RFC
            };
        }

        public InstitucionInfo ObtenerInfo()
        {
            var def = new InstitucionInfo();
            var ctx = _tenantContextAccessor.TenantContext;
            if (ctx == null)
                return def;

            var s = ctx.Settings;
            var configurado = !string.IsNullOrWhiteSpace(s.Direccion) || !string.IsNullOrWhiteSpace(s.RFC);

            return new InstitucionInfo
            {
                Nombre = configurado && !string.IsNullOrWhiteSpace(ctx.Nombre) ? ctx.Nombre : def.Nombre,
                NombreCorto = configurado && !string.IsNullOrWhiteSpace(ctx.Codigo) ? ctx.Codigo : def.NombreCorto,
                Direccion = !string.IsNullOrWhiteSpace(s.Direccion) ? s.Direccion : def.Direccion,
                Telefono = !string.IsNullOrWhiteSpace(s.Telefono) ? s.Telefono : def.Telefono,
                Email = !string.IsNullOrWhiteSpace(s.Email) ? s.Email : def.Email,
                RFC = !string.IsNullOrWhiteSpace(s.RFC) ? s.RFC : def.RFC
            };
        }
    }
}
