using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Models;
using WebApplication2.Core.Requests.BecaCatalogo;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/becas/catalogo")]
    [ApiController]
    [Authorize]
    public class BecaCatalogoController : ControllerBase
    {
        private readonly IBecaCatalogoService _catalogoService;

        public BecaCatalogoController(IBecaCatalogoService catalogoService)
        {
            _catalogoService = catalogoService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Beca>>> ObtenerTodas(
            [FromQuery] bool? soloActivas = null,
            CancellationToken ct = default)
        {
            try
            {
                var becas = await _catalogoService.ObtenerTodasAsync(soloActivas, ct);
                return Ok(becas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Beca>> ObtenerPorId(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            try
            {
                var beca = await _catalogoService.ObtenerPorIdAsync(id, ct);

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

        [HttpPost]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS}")]
        public async Task<ActionResult<Beca>> Crear(
            [FromBody] CrearBecaCatalogoRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var beca = await _catalogoService.CrearAsync(
                    request.Clave,
                    request.Nombre,
                    request.Descripcion,
                    request.Tipo,
                    request.Valor,
                    request.TopeMensual,
                    request.IdConceptoPago,
                    ct);

                return CreatedAtAction(nameof(ObtenerPorId), new { id = beca.IdBeca }, beca);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS}")]
        public async Task<ActionResult<Beca>> Actualizar(
            [FromRoute] int id,
            [FromBody] ActualizarBecaCatalogoRequest request,
            CancellationToken ct = default)
        {
            try
            {
                var beca = await _catalogoService.ActualizarAsync(
                    id,
                    request.Nombre,
                    request.Descripcion,
                    request.Tipo,
                    request.Valor,
                    request.TopeMensual,
                    request.IdConceptoPago,
                    request.Activo,
                    ct);

                return Ok(beca);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPatch("{id}/desactivar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS}")]
        public async Task<ActionResult> Desactivar(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _catalogoService.DesactivarAsync(id, ct);

                if (!resultado)
                {
                    return NotFound(new { Error = $"No se encontró la beca con ID {id}" });
                }

                return Ok(new { Mensaje = "Beca desactivada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPatch("{id}/activar")]
        [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.FINANZAS}")]
        public async Task<ActionResult> Activar(
            [FromRoute] int id,
            CancellationToken ct = default)
        {
            try
            {
                var resultado = await _catalogoService.ActivarAsync(id, ct);

                if (!resultado)
                {
                    return NotFound(new { Error = $"No se encontró la beca con ID {id}" });
                }

                return Ok(new { Mensaje = "Beca activada exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
