using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs.MultiTenant;
using WebApplication2.Services.MultiTenant;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/branding")]
public class BrandingController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public BrandingController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet("current")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TenantPublicInfoDto), StatusCodes.Status200OK)]
    public ActionResult<TenantPublicInfoDto> GetCurrent()
    {
        var tenant = _tenantService.GetCurrentTenant();

        if (tenant == null)
        {
            return Ok(new TenantPublicInfoDto
            {
                Codigo = "",
                Nombre = "SACI",
                NombreCorto = "SACI",
                LogoUrl = null,
                ColorPrimario = "#14356F"
            });
        }

        return Ok(new TenantPublicInfoDto
        {
            Codigo = tenant.Codigo,
            Nombre = tenant.Nombre,
            NombreCorto = tenant.Nombre,
            LogoUrl = tenant.Settings?.LogoUrl,
            ColorPrimario = tenant.Settings?.ColorPrimario ?? "#14356F",
            ColorSecundario = tenant.Settings?.ColorSecundario,
            IncluyeReportes = tenant.Settings?.IncluyeReportes ?? true,
            IncluyeApi = tenant.Settings?.IncluyeApi ?? true,
            IncluyeFacturacion = tenant.Settings?.IncluyeFacturacion ?? true,
            IncluyeSoporte = tenant.Settings?.IncluyeSoporte ?? true
        });
    }
}
