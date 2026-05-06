using WebApplication2.Core.Models.MultiTenant;
using WebApplication2.Services.MultiTenant;

namespace WebApplication2.Middleware;


public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;
    private readonly string _baseDomain;

    private static readonly string[] ExcludedPaths = new[]
    {
        "/health",
        "/swagger",
        "/api/admin",
        "/api/superadmin",
        "/api/email",
        "/api/onlyoffice",
        "/.well-known",
        "/favicon.ico"
    };

    private const string DefaultTenantCode = "USAG";

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _baseDomain = configuration["BaseDomain"] ?? "saciusag.com.mx";
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (IsExcludedPath(path))
        {
            await _next(context);
            return;
        }

        var tenant = await ResolveTenantAsync(context, tenantService);

        if (tenant == null)
        {
            _logger.LogWarning("No se pudo resolver el tenant para: {Host}{Path}",
                context.Request.Host, path);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Institución no identificada",
                message = "No se pudo determinar la institución. Verifica la URL.",
                code = "TENANT_NOT_FOUND"
            });
            return;
        }

        switch (tenant.Status)
        {
            case TenantStatus.Suspended:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Cuenta suspendida",
                    message = "La cuenta de esta institución está suspendida. Contacta al administrador.",
                    code = "TENANT_SUSPENDED"
                });
                return;

            case TenantStatus.Maintenance:
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "En mantenimiento",
                    message = "El sistema está en mantenimiento. Por favor intenta más tarde.",
                    code = "TENANT_MAINTENANCE"
                });
                return;

            case TenantStatus.Inactive:
            case TenantStatus.Pending:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Institución no disponible",
                    message = "Esta institución no está activa actualmente.",
                    code = "TENANT_INACTIVE"
                });
                return;
        }

        tenantService.SetCurrentTenant(new TenantContext
        {
            IdTenant = tenant.IdTenant,
            Codigo = tenant.Codigo,
            Nombre = tenant.Nombre,
            Subdominio = tenant.Subdominio,
            ConnectionString = tenant.ConnectionString,
            Status = tenant.Status,
            Settings = new TenantSettings
            {
                LogoUrl = tenant.LogoUrl,
                ColorPrimario = tenant.ColorPrimario,
                ColorSecundario = tenant.ColorSecundario,
                Timezone = tenant.Timezone,
                MaxEstudiantes = tenant.MaximoEstudiantes,
                MaxUsuarios = tenant.MaximoUsuarios
            }
        });

        context.Response.Headers.Append("X-Tenant-Id", tenant.IdTenant.ToString());
        context.Response.Headers.Append("X-Tenant-Code", tenant.Codigo);

        _logger.LogDebug("Tenant establecido: {TenantCode} ({TenantId}) para {Path}",
            tenant.Codigo, tenant.IdTenant, path);

        await _next(context);
    }

    private async Task<Tenant?> ResolveTenantAsync(HttpContext context, ITenantService tenantService)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            if (int.TryParse(tenantIdHeader, out int tenantId))
            {
                _logger.LogDebug("Tenant resuelto por header X-Tenant-Id: {TenantId}", tenantId);
                return await tenantService.GetTenantByIdAsync(tenantId);
            }
        }

        if (context.Request.Headers.TryGetValue("X-Tenant-Code", out var tenantCodeHeader))
        {
            _logger.LogDebug("Tenant resuelto por header X-Tenant-Code: {TenantCode}", tenantCodeHeader.ToString());
            return await tenantService.GetTenantBySubdomainAsync(tenantCodeHeader!);
        }

        var host = context.Request.Host.Host;
        var subdomain = ExtractSubdomain(host);

        if (!string.IsNullOrEmpty(subdomain))
        {
            _logger.LogDebug("Tenant resuelto por subdominio: {Subdomain} (host: {Host})", subdomain, host);
            return await tenantService.GetTenantBySubdomainAsync(subdomain);
        }

        if (IsRootDomain(host))
        {
            _logger.LogDebug("Dominio raíz detectado: {Host} → Usando tenant por defecto: {DefaultTenant}", host, DefaultTenantCode);
            return await tenantService.GetTenantBySubdomainAsync(DefaultTenantCode.ToLower());
        }

        _logger.LogDebug("Intentando resolver por dominio completo: {Host}", host);
        return await tenantService.GetTenantBySubdomainAsync(host);
    }

    private static readonly HashSet<string> ReservedSubdomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "www", "api", "admin", "test", "test-api", "staging", "dev"
    };

    private bool IsRootDomain(string host)
    {
        host = host.Split(':')[0].ToLower();

        if (host == _baseDomain || host == "localhost" || host == "127.0.0.1")
            return true;

        if (host.EndsWith($".{_baseDomain}"))
        {
            var subdomain = host.Replace($".{_baseDomain}", "");
            return ReservedSubdomains.Contains(subdomain);
        }

        return false;
    }

    private string? ExtractSubdomain(string host)
    {
        host = host.Split(':')[0].ToLower();

        if (host.Contains("localhost") || host == "127.0.0.1")
        {
            return null;
        }

        if (host == _baseDomain)
        {
            return null;
        }

        if (host.EndsWith($".{_baseDomain}"))
        {
            var subdomain = host.Replace($".{_baseDomain}", "");
            if (!ReservedSubdomains.Contains(subdomain))
            {
                return subdomain;
            }
            return null;
        }

        return null;
    }

    private bool IsExcludedPath(string path)
    {
        return ExcludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase));
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantMiddleware>();
    }
}
