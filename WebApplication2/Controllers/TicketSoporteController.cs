using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.Ticket;
using WebApplication2.Core.Enums;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    [Authorize]
    public class TicketSoporteController : ControllerBase
    {
        private readonly ITicketSoporteService _ticketService;

        public TicketSoporteController(ITicketSoporteService ticketService)
        {
            _ticketService = ticketService;
        }

        private string GetUserId() => User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException();
        private string GetUserName() => User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario";
        private bool EsAdmin() => User.IsInRole("admin") || User.IsInRole("superadmin");

        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] CrearTicketDto dto, IFormFile? archivo)
        {
            try
            {
                var result = await _ticketService.CrearAsync(dto, archivo, GetUserId(), GetUserName(), EsAdmin());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] TicketFiltroDto filtro)
        {
            try
            {
                var result = await _ticketService.ListarAsync(filtro, GetUserId(), EsAdmin());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var result = await _ticketService.ObtenerPorIdAsync(id, GetUserId(), EsAdmin());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarTicketDto dto)
        {
            try
            {
                var result = await _ticketService.ActualizarAsync(id, dto, GetUserId(), EsAdmin());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/comentarios")]
        public async Task<IActionResult> AgregarComentario(int id, [FromForm] CrearComentarioTicketDto dto, IFormFile? archivo)
        {
            try
            {
                var result = await _ticketService.AgregarComentarioAsync(id, dto, archivo, GetUserId(), GetUserName(), EsAdmin());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/estatus")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.SUPER_ADMIN}")]
        public async Task<IActionResult> CambiarEstatus(int id, [FromBody] CambiarEstatusTicketRequest request)
        {
            try
            {
                await _ticketService.CambiarEstatusAsync(id, request.Estatus, GetUserId());
                return Ok(new { message = "Estatus actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/asignar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.SUPER_ADMIN}")]
        public async Task<IActionResult> Asignar(int id, [FromBody] AsignarTicketRequest request)
        {
            try
            {
                await _ticketService.AsignarAsync(id, request.UsuarioAsignadoId, request.NombreAsignado);
                return Ok(new { message = "Ticket asignado correctamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("estadisticas")]
        public async Task<IActionResult> Estadisticas()
        {
            try
            {
                var result = await _ticketService.ObtenerEstadisticasAsync(GetUserId(), EsAdmin());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CambiarEstatusTicketRequest
    {
        public TicketEstatusEnum Estatus { get; set; }
    }

    public class AsignarTicketRequest
    {
        public string UsuarioAsignadoId { get; set; } = string.Empty;
        public string NombreAsignado { get; set; } = string.Empty;
    }
}
