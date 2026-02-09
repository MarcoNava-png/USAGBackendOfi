using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication2.Core.DTOs.MultiTenant;
using WebApplication2.Core.Models.MultiTenant;
using WebApplication2.Core.Requests.MultiTenant;
using WebApplication2.Core.Responses.MultiTenant;
using WebApplication2.Data.DbContexts;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/superadmin/auth")]
public class SuperAdminAuthController : ControllerBase
{
    private readonly MasterDbContext _masterDb;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SuperAdminAuthController> _logger;

    public SuperAdminAuthController(
        MasterDbContext masterDb,
        IConfiguration configuration,
        ILogger<SuperAdminAuthController> logger)
    {
        _masterDb = masterDb;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginSuperAdminResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginSuperAdminResponse>> Login(
        [FromBody] LoginSuperAdminRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new LoginSuperAdminResponse
            {
                Exitoso = false,
                Mensaje = "Email y contraseña son requeridos"
            });
        }

        var superAdmin = await _masterDb.SuperAdmins
            .Include(s => s.TenantAccess)
            .FirstOrDefaultAsync(s => s.Email.ToLower() == request.Email.ToLower(), ct);

        if (superAdmin == null)
        {
            _logger.LogWarning("Intento de login fallido: SuperAdmin no encontrado - {Email}", request.Email);
            return Unauthorized(new LoginSuperAdminResponse
            {
                Exitoso = false,
                Mensaje = "Credenciales inválidas"
            });
        }

        if (!superAdmin.Activo)
        {
            _logger.LogWarning("Intento de login de SuperAdmin inactivo: {Email}", request.Email);
            return Unauthorized(new LoginSuperAdminResponse
            {
                Exitoso = false,
                Mensaje = "Esta cuenta está desactivada"
            });
        }

        var hasher = new PasswordHasher<SuperAdmin>();
        var result = hasher.VerifyHashedPassword(superAdmin, superAdmin.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Intento de login fallido: Contraseña incorrecta - {Email}", request.Email);
            return Unauthorized(new LoginSuperAdminResponse
            {
                Exitoso = false,
                Mensaje = "Credenciales inválidas"
            });
        }

        superAdmin.LastLoginAt = DateTime.UtcNow;
        superAdmin.LastLoginIP = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _masterDb.SaveChangesAsync(ct);

        var token = GenerateJwtToken(superAdmin);

        _logger.LogInformation("SuperAdmin login exitoso: {Email}", request.Email);

        return Ok(new LoginSuperAdminResponse
        {
            Exitoso = true,
            Token = token,
            Mensaje = "Login exitoso",
            SuperAdmin = new SuperAdminInfoDto
            {
                IdSuperAdmin = superAdmin.IdSuperAdmin,
                Email = superAdmin.Email,
                NombreCompleto = superAdmin.NombreCompleto,
                AccesoTotal = superAdmin.AccesoTotal,
                TenantsAcceso = superAdmin.TenantAccess.Select(t => t.IdTenant).ToList()
            }
        });
    }

    [HttpGet("verify")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(typeof(SuperAdminInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuperAdminInfoDto>> VerifyToken(CancellationToken ct)
    {
        var superAdminIdClaim = User.FindFirst("superAdminId")?.Value;
        if (string.IsNullOrEmpty(superAdminIdClaim) || !int.TryParse(superAdminIdClaim, out int superAdminId))
        {
            return Unauthorized();
        }

        var superAdmin = await _masterDb.SuperAdmins
            .Include(s => s.TenantAccess)
            .FirstOrDefaultAsync(s => s.IdSuperAdmin == superAdminId, ct);

        if (superAdmin == null || !superAdmin.Activo)
        {
            return Unauthorized();
        }

        return Ok(new SuperAdminInfoDto
        {
            IdSuperAdmin = superAdmin.IdSuperAdmin,
            Email = superAdmin.Email,
            NombreCompleto = superAdmin.NombreCompleto,
            AccesoTotal = superAdmin.AccesoTotal,
            TenantsAcceso = superAdmin.TenantAccess.Select(t => t.IdTenant).ToList()
        });
    }

    [HttpPost("create")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateSuperAdmin(
        [FromBody] CrearSuperAdminRequest request,
        CancellationToken ct)
    {
        var currentIdClaim = User.FindFirst("superAdminId")?.Value;
        if (string.IsNullOrEmpty(currentIdClaim) || !int.TryParse(currentIdClaim, out int currentId))
        {
            return Unauthorized();
        }

        var currentAdmin = await _masterDb.SuperAdmins.FindAsync(currentId, ct);
        if (currentAdmin == null || !currentAdmin.AccesoTotal)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.NombreCompleto) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email, nombre y contraseña son requeridos" });
        }

        var exists = await _masterDb.SuperAdmins.AnyAsync(s => s.Email.ToLower() == request.Email.ToLower(), ct);
        if (exists)
        {
            return BadRequest(new { error = "Ya existe un SuperAdmin con ese email" });
        }

        var hasher = new PasswordHasher<SuperAdmin>();
        var newAdmin = new SuperAdmin
        {
            Email = request.Email,
            NombreCompleto = request.NombreCompleto,
            AccesoTotal = request.AccesoTotal,
            Activo = true,
            CreatedAt = DateTime.UtcNow
        };
        newAdmin.PasswordHash = hasher.HashPassword(newAdmin, request.Password);

        _masterDb.SuperAdmins.Add(newAdmin);
        await _masterDb.SaveChangesAsync(ct);

        _logger.LogInformation("SuperAdmin creado: {Email} por {CreatorEmail}",
            request.Email, currentAdmin.Email);

        return Ok(new
        {
            mensaje = "SuperAdmin creado exitosamente",
            idSuperAdmin = newAdmin.IdSuperAdmin
        });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { error = "Email y nueva contraseña son requeridos" });
        }

        var superAdmin = await _masterDb.SuperAdmins
            .FirstOrDefaultAsync(s => s.Email.ToLower() == request.Email.ToLower(), ct);

        if (superAdmin == null)
        {
            return NotFound(new { error = "SuperAdmin no encontrado" });
        }

        var hasher = new PasswordHasher<SuperAdmin>();
        superAdmin.PasswordHash = hasher.HashPassword(superAdmin, request.NewPassword);
        await _masterDb.SaveChangesAsync(ct);

        _logger.LogWarning("Contraseña reseteada para SuperAdmin: {Email}", request.Email);

        return Ok(new { mensaje = "Contraseña actualizada exitosamente" });
    }

    [HttpPost("seed")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SeedFirstSuperAdmin(
        [FromBody] CrearSuperAdminRequest request,
        CancellationToken ct)
    {
        var existsAny = await _masterDb.SuperAdmins.AnyAsync(ct);
        if (existsAny)
        {
            return BadRequest(new { error = "Ya existe al menos un SuperAdmin. Use el endpoint /create con autenticación." });
        }

        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.NombreCompleto) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email, nombre y contraseña son requeridos" });
        }

        if (request.Password.Length < 8)
        {
            return BadRequest(new { error = "La contraseña debe tener al menos 8 caracteres" });
        }

        var hasher = new PasswordHasher<SuperAdmin>();
        var newAdmin = new SuperAdmin
        {
            Email = request.Email,
            NombreCompleto = request.NombreCompleto,
            AccesoTotal = true,
            Activo = true,
            CreatedAt = DateTime.UtcNow
        };
        newAdmin.PasswordHash = hasher.HashPassword(newAdmin, request.Password);

        _masterDb.SuperAdmins.Add(newAdmin);
        await _masterDb.SaveChangesAsync(ct);

        _logger.LogInformation("Primer SuperAdmin creado: {Email}", request.Email);

        return Ok(new
        {
            mensaje = "SuperAdmin inicial creado exitosamente",
            idSuperAdmin = newAdmin.IdSuperAdmin,
            email = newAdmin.Email
        });
    }

    private string GenerateJwtToken(SuperAdmin superAdmin)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["Jwt:Issuer"];
        var jwtAudience = _configuration["Jwt:Audience"];

        var claims = new[]
        {
            new Claim("superAdminId", superAdmin.IdSuperAdmin.ToString()),
            new Claim(ClaimTypes.Email, superAdmin.Email),
            new Claim(ClaimTypes.Name, superAdmin.NombreCompleto),
            new Claim(ClaimTypes.Role, "SuperAdmin"),
            new Claim("accesoTotal", superAdmin.AccesoTotal.ToString().ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
