using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.Beca;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/becas")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS},{Rol.CONTROL_ESCOLAR}")]
    public class BecaController : ControllerBase
    {
        private readonly IBecaService _becaService;

        public BecaController(IBecaService becaService)
        {
            _becaService = becaService;
        }

        [HttpPost("asignar")]
        public async Task<ActionResult<BecaAsignacion>> AsignarBeca(
            [FromBody] AsignarBecaRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var beca = await _becaService.AsignarBecaAsync(
                    request.IdEstudiante,
                    request.IdConceptoPago,
                    request.Tipo,
                    request.Valor,
                    request.VigenciaDesde,
                    request.VigenciaHasta,
                    request.TopeMensual,
                    request.Observaciones,
                    ct);

                return CreatedAtAction(nameof(ObtenerBeca), new { id = beca.IdBecaAsignacion }, beca);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("asignar-catalogo")]
        public async Task<ActionResult<BecaAsignacion>> AsignarBecaDesdeCatalogo(
            [FromBody] AsignarBecaCatalogoRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var beca = await _becaService.AsignarBecaDesdeCatalogoAsync(
                    request.IdEstudiante,
                    request.IdBeca,
                    request.VigenciaDesde,
                    request.VigenciaHasta,
                    request.Observaciones,
                    request.IdPeriodoAcademico,
                    ct);

                return CreatedAtAction(nameof(ObtenerBeca), new { id = beca.IdBecaAsignacion }, beca);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BecaAsignacion>> ActualizarBeca(
            [FromRoute] long id,
            [FromBody] ActualizarBecaRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var beca = await _becaService.ActualizarBecaAsignacionAsync(
                    id,
                    request.VigenciaDesde,
                    request.VigenciaHasta,
                    request.Observaciones,
                    request.Activo,
                    request.IdPeriodoAcademico,
                    ct);

                if (beca == null)
                {
                    return NotFound(new { Error = $"No se encontró la beca con ID {id}" });
                }

                return Ok(beca);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("estudiante/{idEstudiante}")]
        public async Task<ActionResult<IReadOnlyList<BecaAsignacion>>> ObtenerBecasEstudiante(
            [FromRoute] int idEstudiante,
            [FromQuery] bool? soloActivas = true,
            CancellationToken ct = default)
        {
            try
            {
                var becas = await _becaService.ObtenerBecasEstudianteAsync(idEstudiante, soloActivas, ct);
                return Ok(becas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("estudiante/{idEstudiante}/activas")]
        public async Task<ActionResult<IReadOnlyList<BecaAsignacion>>> ObtenerBecasActivas(
            [FromRoute] int idEstudiante,
            [FromQuery] DateOnly? fecha = null,
            [FromQuery] int? idConceptoPago = null,
            CancellationToken ct = default)
        {
            try
            {
                var fechaConsulta = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
                var becas = await _becaService.ObtenerBecasActivasAsync(idEstudiante, fechaConsulta, idConceptoPago, ct);
                return Ok(becas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BecaAsignacion>> ObtenerBeca(
            [FromRoute] long id,
            CancellationToken ct = default)
        {
            try
            {
                var beca = await _becaService.ObtenerBecaPorIdAsync(id, ct);

                if (beca == null)
                {
                    return NotFound(new { Error = $"No se encontró la beca con ID {id}" });
                }

                return Ok(beca);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> CancelarBeca(
            [FromRoute] long id,
            CancellationToken ct = default)
        {
            try
            {
                var cancelada = await _becaService.CancelarBecaAsync(id, ct);

                if (!cancelada)
                {
                    return NotFound(new { Error = $"No se encontró la beca con ID {id}" });
                }

                return Ok(new { Mensaje = "Beca cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("calcular-descuento")]
        public async Task<ActionResult<object>> CalcularDescuento(
            [FromBody] CalcularDescuentoRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var descuento = await _becaService.CalcularDescuentoPorBecasAsync(
                    request.IdEstudiante,
                    request.IdConceptoPago,
                    request.ImporteBase,
                    request.FechaAplicacion ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    ct);

                var total = request.ImporteBase - descuento;

                return Ok(new
                {
                    importeBase = request.ImporteBase,
                    descuento = descuento,
                    porcentajeDescuento = request.ImporteBase > 0
                        ? Math.Round((descuento / request.ImporteBase) * 100, 2)
                        : 0,
                    total = total
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("recalcular-descuentos")]
        public async Task<ActionResult<object>> RecalcularDescuentos(
            [FromBody] RecalcularDescuentosRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var recibosActualizados = await _becaService.RecalcularDescuentosRecibosAsync(
                    request.IdEstudiante,
                    request.IdPeriodoAcademico,
                    ct);

                return Ok(new
                {
                    mensaje = "Descuentos recalculados exitosamente",
                    recibosActualizados = recibosActualizados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
