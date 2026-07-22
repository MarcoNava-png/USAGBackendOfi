using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs.Formatos;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/mis-formatos")]
    [ApiController]
    [Authorize]
    public class MisFormatosController : ControllerBase
    {
        private readonly IPlantillaReporteService _service;

        public MisFormatosController(IPlantillaReporteService service)
        {
            _service = service;
        }

        private IEnumerable<string> RolesActuales() =>
            User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        [HttpGet]
        public async Task<ActionResult<List<MiFormatoDto>>> MisFormatos(CancellationToken ct)
        {
            var formatos = await _service.ListarPorRolesAsync(RolesActuales(), ct);
            return Ok(formatos.Select(p => new MiFormatoDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Codigo = p.Codigo,
                Descripcion = p.Descripcion,
                Origen = p.Origen,
                Categoria = p.Categoria
            }).ToList());
        }

        [HttpPost("{codigo}/generar")]
        public async Task<IActionResult> Generar(string codigo, CancellationToken ct)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var pdf = await _service.GenerarMioAsync(codigo, userId, RolesActuales(), ct);
                return File(pdf, "application/pdf", $"{codigo}.pdf");
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
