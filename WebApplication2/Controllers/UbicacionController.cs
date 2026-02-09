using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.Models;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UbicacionController : ControllerBase
    {
        private readonly IUbicacionService _ubicacionService;

        public UbicacionController(IUbicacionService ubicacionService)
        {
            _ubicacionService = ubicacionService;
        }

        [HttpGet("estados")]
        public async Task<ActionResult<IEnumerable<Estado>>> GetEstados()
        {
            var estados = await _ubicacionService.ObtenerEstados();

            return Ok(estados);
        }

        [HttpGet("municipios/{estadoId}")]
        public async Task<ActionResult<IEnumerable<Municipio>>> GetMunicipios(string estadoId)
        {
            var municipios = await _ubicacionService.ObtenerMunicipios(estadoId);

            return Ok(municipios);
        }

        [HttpGet("asentamientos/{municipioId}")]
        public async Task<ActionResult<IEnumerable<Municipio>>> GetAsentamientos(string municipioId)
        {
            var asentamientos = await _ubicacionService.ObtenerCodigosPostales(municipioId);

            return Ok(asentamientos);
        }
    }
}
