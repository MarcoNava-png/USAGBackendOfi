using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs.MicrosoftGraph;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.MicrosoftGraph;
using WebApplication2.Core.Responses.MicrosoftGraph;
using WebApplication2.Services;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailController : ControllerBase
{
    private readonly IMicrosoftGraphService _graphService;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailController> _logger;

    public EmailController(
        IMicrosoftGraphService graphService,
        IAuthService authService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<EmailController> logger)
    {
        _graphService = graphService;
        _authService = authService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("{userEmail}")]
    [ProducesResponseType(typeof(List<EmailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EmailDto>>> GetEmails(
        string userEmail,
        [FromQuery] int top = 50,
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default)
    {
        try
        {
            var emails = await _graphService.GetEmailsAsync(userEmail, top, unreadOnly, ct);
            return Ok(emails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener correos de {UserEmail}", userEmail);
            return StatusCode(500, new { error = $"Error al obtener correos: {ex.Message}" });
        }
    }

    [HttpGet("{userEmail}/messages/{messageId}")]
    [ProducesResponseType(typeof(EmailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailDto>> GetEmail(
        string userEmail,
        string messageId,
        CancellationToken ct)
    {
        try
        {
            var email = await _graphService.GetEmailByIdAsync(userEmail, messageId, ct);
            if (email == null)
                return NotFound(new { error = "Correo no encontrado" });

            return Ok(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener correo {MessageId}", messageId);
            return StatusCode(500, new { error = $"Error al obtener correo: {ex.Message}" });
        }
    }

    [HttpPost("{userEmail}/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> SendEmail(
        string userEmail,
        [FromBody] SendEmailRequest request,
        CancellationToken ct)
    {
        try
        {
            await _graphService.SendEmailAsync(userEmail, request, ct);
            return Ok(new { mensaje = "Correo enviado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo desde {UserEmail}", userEmail);
            return StatusCode(500, new { error = $"Error al enviar correo: {ex.Message}" });
        }
    }

    [HttpPost("{userEmail}/messages/{messageId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> MarkAsRead(
        string userEmail,
        string messageId,
        CancellationToken ct)
    {
        try
        {
            await _graphService.MarkAsReadAsync(userEmail, messageId, ct);
            return Ok(new { mensaje = "Correo marcado como leido" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar correo como leido {MessageId}", messageId);
            return StatusCode(500, new { error = $"Error: {ex.Message}" });
        }
    }

    [HttpGet("{userEmail}/search")]
    [ProducesResponseType(typeof(List<EmailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EmailDto>>> SearchEmails(
        string userEmail,
        [FromQuery] string query,
        [FromQuery] int top = 50,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { error = "El parametro 'query' es requerido" });

            var emails = await _graphService.SearchEmailsAsync(userEmail, query, top, ct);
            return Ok(emails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar correos de {UserEmail}", userEmail);
            return StatusCode(500, new { error = $"Error: {ex.Message}" });
        }
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(List<UserInfoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserInfoDto>>> GetUsers(
        [FromQuery] int top = 100,
        CancellationToken ct = default)
    {
        try
        {
            var users = await _graphService.GetUsersAsync(top, ct);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios del directorio");
            return StatusCode(500, new { error = $"Error: {ex.Message}" });
        }
    }

    [HttpGet("users/{userIdOrEmail}")]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserInfoDto>> GetUserById(
        string userIdOrEmail,
        CancellationToken ct = default)
    {
        try
        {
            var user = await _graphService.GetUserByIdAsync(userIdOrEmail, ct);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {UserIdOrEmail}", userIdOrEmail);
            return StatusCode(500, new { error = $"Error: {ex.Message}" });
        }
    }

    [HttpPost("users")]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateUserResponse>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.DisplayName))
                return BadRequest(new { error = "DisplayName es requerido" });
            if (string.IsNullOrWhiteSpace(request.UserPrincipalName))
                return BadRequest(new { error = "UserPrincipalName es requerido" });
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Password es requerido" });

            var result = await _graphService.CreateUserAsync(request, ct);

            if (result.Success && request.CreateInApp && request.Roles.Count > 0)
            {
                try
                {
                    var appUser = new ApplicationUser
                    {
                        UserName = request.UserPrincipalName,
                        Email = request.UserPrincipalName,
                        Nombres = request.GivenName ?? "",
                        Apellidos = request.Surname ?? "",
                        Telefono = request.MobilePhone,
                    };

                    var createdUser = await _authService.Signup(appUser, request.Password, request.Roles);
                    result.AppUserCreated = true;
                    result.AppUserId = createdUser.Id;
                    result.AssignedRoles = request.Roles;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Usuario creado en Azure AD pero fallo la creacion en SACI: {Email}", request.UserPrincipalName);
                    result.AppUserCreated = false;
                    result.Message = $"Correo creado en Azure, pero hubo un error al crear en el sistema: {ex.Message}";
                }
            }

            if (result.Success && request.SendCredentialsEmail)
            {
                try
                {
                    var frontendUrl = _configuration["FrontendUrl"] ?? "https://saciusag.com.mx";
                    var loginUrl = $"{frontendUrl}/auth/v2/login";
                    var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #2563eb, #1d4ed8); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f8fafc; padding: 30px; border: 1px solid #e2e8f0; }}
        .credentials {{ background: #ffffff; border: 2px solid #e2e8f0; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .credentials p {{ margin: 8px 0; }}
        .label {{ color: #64748b; font-size: 13px; }}
        .value {{ font-family: monospace; font-size: 16px; font-weight: bold; color: #1e293b; }}
        .button {{ display: inline-block; background: #2563eb; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #64748b; font-size: 12px; }}
        .warning {{ background: #fef3c7; border: 1px solid #f59e0b; border-radius: 8px; padding: 12px; margin: 15px 0; color: #92400e; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Bienvenido al Sistema SACI</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{request.DisplayName}</strong>,</p>
            <p>Se ha creado tu cuenta institucional. A continuacion encontraras tus credenciales de acceso:</p>
            <div class='credentials'>
                <p><span class='label'>Correo institucional:</span><br><span class='value'>{request.UserPrincipalName}</span></p>
                <p><span class='label'>Contrasena temporal:</span><br><span class='value'>{request.Password}</span></p>
            </div>
            <div class='warning'>
                <strong>Importante:</strong> Deberas cambiar tu contrasena la primera vez que inicies sesion.
            </div>
            <p>Ingresa al sistema haciendo clic en el siguiente boton:</p>
            <p style='text-align: center;'>
                <a href='{loginUrl}' class='button'>Iniciar Sesion</a>
            </p>
            <p>O copia y pega este enlace en tu navegador:</p>
            <p style='word-break: break-all; color: #2563eb;'>{loginUrl}</p>
        </div>
        <div class='footer'>
            <p>Sistema de Gestion Escolar SACI - USAG</p>
            <p>Este es un correo automatico, por favor no responder.</p>
        </div>
    </div>
</body>
</html>";

                    await _emailService.SendEmailAsync(
                        request.UserPrincipalName,
                        "Bienvenido al Sistema SACI - Tus credenciales de acceso",
                        htmlBody);

                    result.CredentialsEmailSent = true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar correo de credenciales a {Email}", request.UserPrincipalName);
                    result.CredentialsEmailSent = false;
                }
            }

            return Created($"/api/email/users/{result.UserId}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario {DisplayName}", request.DisplayName);
            return StatusCode(500, new { error = $"Error al crear usuario: {ex.Message}" });
        }
    }

    [HttpPut("users/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateUser(
        string userId,
        [FromBody] CreateUserRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var success = await _graphService.UpdateUserAsync(userId, request, ct);
            if (!success)
                return NotFound(new { error = "Usuario no encontrado" });

            return Ok(new { mensaje = "Usuario actualizado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {UserId}", userId);
            return StatusCode(500, new { error = $"Error: {ex.Message}" });
        }
    }

    [HttpDelete("users/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUser(
        string userId,
        CancellationToken ct = default)
    {
        try
        {
            var success = await _graphService.DeleteUserAsync(userId, ct);
            if (!success)
                return NotFound(new { error = "Usuario no encontrado" });

            return Ok(new { mensaje = "Usuario eliminado exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario {UserId}", userId);
            return StatusCode(500, new { error = $"Error: {ex.Message}" });
        }
    }

    [HttpPost("users/{userId}/reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ResetPassword(
        string userId,
        CancellationToken ct = default)
    {
        try
        {
            var newPassword = await _graphService.ResetPasswordAsync(userId, ct);
            return Ok(new { mensaje = "Contraseña restablecida exitosamente", password = newPassword });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resetear contraseña de usuario {UserId}", userId);
            return StatusCode(500, new { error = $"Error: {ex.Message}" });
        }
    }

    [HttpGet("domains")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> GetDomains(CancellationToken ct = default)
    {
        try
        {
            var domains = await _graphService.GetDomainsAsync(ct);
            return Ok(domains);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener dominios");
            return StatusCode(500, new { error = $"Error: {ex.Message}" });
        }
    }
}
