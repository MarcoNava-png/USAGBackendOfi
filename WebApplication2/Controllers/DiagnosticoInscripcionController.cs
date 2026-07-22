using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.DTOs.Diagnostico;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/diagnostico-inscripcion")]
    [Authorize(Roles = $"{Rol.SUPER_ADMIN},{Rol.ADMIN},{Rol.CONTROL_ESCOLAR}")]
    public class DiagnosticoInscripcionController : ControllerBase
    {
        private readonly IDiagnosticoInscripcionService _service;

        public DiagnosticoInscripcionController(IDiagnosticoInscripcionService service)
        {
            _service = service;
        }

        [HttpGet("{idPeriodoAcademico:int}")]
        public async Task<ActionResult<List<InconsistenciaInscripcionDto>>> Detectar(int idPeriodoAcademico)
        {
            return Ok(await _service.DetectarAsync(idPeriodoAcademico));
        }

        [HttpPost("reparar")]
        public async Task<ActionResult<RepararInscripcionResultDto>> Reparar([FromBody] RepararInscripcionRequest req)
        {
            return Ok(await _service.RepararAsync(req));
        }
    }
}
