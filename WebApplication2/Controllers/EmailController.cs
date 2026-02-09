using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs.MicrosoftGraph;
using WebApplication2.Core.Requests.MicrosoftGraph;
using WebApplication2.Core.Responses.MicrosoftGraph;
using WebApplication2.Services;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmailController : ControllerBase
{
    private readonly IMicrosoftGraphService _graphService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(
        IMicrosoftGraphService graphService,
        ILogger<EmailController> logger)
    {
        _graphService = graphService;
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
