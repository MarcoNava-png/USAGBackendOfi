using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Core.DTOs;
using WebApplication2.Core.DTOs.Importacion;
using WebApplication2.Core.Requests.Importacion;
using WebApplication2.Core.Responses.Importacion;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImportacionController : ControllerBase
    {
        private readonly IImportacionService _importacionService;

        public ImportacionController(IImportacionService importacionService)
        {
            _importacionService = importacionService;
        }

        [HttpGet("campus")]
        public async Task<ActionResult<List<string>>> GetCampusDisponibles()
        {
            var campus = await _importacionService.GetCampusDisponiblesAsync();
            return Ok(campus);
        }

        [HttpGet("planes")]
        public async Task<ActionResult<List<string>>> GetPlanesDisponibles()
        {
            var planes = await _importacionService.GetPlanesDisponiblesAsync();
            return Ok(planes);
        }

        [HttpPost("estudiantes/validar")]
        public async Task<ActionResult<ValidarImportacionResponse>> ValidarImportacion([FromBody] ValidarImportacionRequest request)
        {
            if (request.Estudiantes == null || request.Estudiantes.Count == 0)
            {
                return BadRequest("No se proporcionaron datos para validar");
            }

            var resultado = await _importacionService.ValidarImportacionAsync(request);
            return Ok(resultado);
        }

        [HttpPost("estudiantes")]
        public async Task<ActionResult<ImportarEstudiantesResponse>> ImportarEstudiantes([FromBody] ImportarEstudiantesRequest request)
        {
            if (request.Estudiantes == null || request.Estudiantes.Count == 0)
            {
                return BadRequest("No se proporcionaron estudiantes para importar");
            }

            if (request.Estudiantes.Count > 1000)
            {
                return BadRequest("Máximo 1000 registros por petición");
            }

            var resultado = await _importacionService.ImportarEstudiantesAsync(request);
            return Ok(resultado);
        }

        [HttpPost("estudiantes/uno")]
        public async Task<ActionResult<ImportarEstudiantesResponse>> ImportarUnEstudiante([FromBody] ImportarEstudianteDto estudiante)
        {
            var request = new ImportarEstudiantesRequest
            {
                Estudiantes = new List<ImportarEstudianteDto> { estudiante },
                ActualizarExistentes = true
            };

            var resultado = await _importacionService.ImportarEstudiantesAsync(request);
            return Ok(resultado);
        }

        [HttpPost("campus")]
        public async Task<ActionResult<ImportarCampusResponse>> ImportarCampus([FromBody] ImportarCampusRequest request)
        {
            if (request.Campus == null || request.Campus.Count == 0)
            {
                return BadRequest("No se proporcionaron campus para importar");
            }

            if (request.Campus.Count > 100)
            {
                return BadRequest("Máximo 100 campus por petición");
            }

            var resultado = await _importacionService.ImportarCampusAsync(request);
            return Ok(resultado);
        }

        [HttpPost("campus/uno")]
        public async Task<ActionResult<ImportarCampusResponse>> ImportarUnCampus([FromBody] ImportarCampusDto campus)
        {
            var request = new ImportarCampusRequest
            {
                Campus = new List<ImportarCampusDto> { campus },
                ActualizarExistentes = true
            };

            var resultado = await _importacionService.ImportarCampusAsync(request);
            return Ok(resultado);
        }

        [HttpPost("planes")]
        public async Task<ActionResult<ImportarPlanesEstudiosResponse>> ImportarPlanesEstudios([FromBody] ImportarPlanesEstudiosRequest request)
        {
            if (request.Planes == null || request.Planes.Count == 0)
            {
                return BadRequest("No se proporcionaron planes de estudio para importar");
            }

            if (request.Planes.Count > 200)
            {
                return BadRequest("Máximo 200 planes por petición");
            }

            var resultado = await _importacionService.ImportarPlanesEstudiosAsync(request);
            return Ok(resultado);
        }

        [HttpPost("planes/uno")]
        public async Task<ActionResult<ImportarPlanesEstudiosResponse>> ImportarUnPlanEstudios([FromBody] ImportarPlanEstudiosDto plan)
        {
            var request = new ImportarPlanesEstudiosRequest
            {
                Planes = new List<ImportarPlanEstudiosDto> { plan },
                ActualizarExistentes = true
            };

            var resultado = await _importacionService.ImportarPlanesEstudiosAsync(request);
            return Ok(resultado);
        }

        [HttpPost("materias/validar")]
        public async Task<ActionResult<ValidarMateriasResponse>> ValidarMaterias([FromBody] ValidarMateriasRequest request)
        {
            if (request.Materias == null || request.Materias.Count == 0)
            {
                return BadRequest("No se proporcionaron materias para validar");
            }

            var resultado = await _importacionService.ValidarMateriasAsync(request);
            return Ok(new { data = resultado });
        }

        [HttpPost("materias")]
        public async Task<ActionResult<ImportarMateriasResponse>> ImportarMaterias([FromBody] ImportarMateriasRequest request)
        {
            if (request.Materias == null || request.Materias.Count == 0)
            {
                return BadRequest("No se proporcionaron materias para importar");
            }

            if (request.Materias.Count > 5000)
            {
                return BadRequest("Máximo 5000 materias por petición");
            }

            var resultado = await _importacionService.ImportarMateriasAsync(request);
            return Ok(new { data = resultado });
        }

        [HttpPost("materias/uno")]
        public async Task<ActionResult<ImportarMateriasResponse>> ImportarUnaMateria([FromBody] ImportarMateriaDto materia)
        {
            var request = new ImportarMateriasRequest
            {
                Materias = new List<ImportarMateriaDto> { materia },
                ActualizarExistentes = true,
                CrearRelacionSiExiste = true
            };

            var resultado = await _importacionService.ImportarMateriasAsync(request);
            return Ok(resultado);
        }

        [HttpGet("materias/plantilla")]
        public async Task<IActionResult> DescargarPlantillaMaterias([FromQuery] int? idPlanEstudios = null)
        {
            try
            {
                var bytes = await _importacionService.GenerarPlantillaMateriasAsync(idPlanEstudios);
                var fileName = idPlanEstudios.HasValue
                    ? $"plantilla_materias_plan_{idPlanEstudios}.xlsx"
                    : "plantilla_materias.xlsx";

                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al generar plantilla: {ex.Message}");
            }
        }
    }
}
