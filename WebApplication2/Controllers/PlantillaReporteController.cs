using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Configuration.Constants;
using WebApplication2.Core.Requests.Formatos;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers
{
    [Route("api/plantillas-reporte")]
    [ApiController]
    [Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.CONTROL_ESCOLAR},{Rol.COORDINADOR},{Rol.ACADEMICO}")]
    public class PlantillaReporteController : ControllerBase
    {
        private readonly IPlantillaReporteService _service;
        private readonly IFormatoDatosService _formatoDatos;
        private readonly ApplicationDbContext _db;

        public PlantillaReporteController(IPlantillaReporteService service, IFormatoDatosService formatoDatos, ApplicationDbContext db)
        {
            _service = service;
            _formatoDatos = formatoDatos;
            _db = db;
        }

        [HttpGet("origenes")]
        public ActionResult ListarOrigenes() => Ok(_formatoDatos.ListarOrigenes());

        [HttpGet("origenes/{origen}/variables")]
        public ActionResult CatalogoVariables(string origen) => Ok(_formatoDatos.CatalogoVariables(origen));

        [HttpPost("{codigo}/generar-entidad/{idEntidad:int}")]
        public async Task<IActionResult> GenerarPorEntidad(string codigo, int idEntidad, [FromQuery] bool pdf = true, CancellationToken ct = default)
        {
            try
            {
                var bytes = await _service.GenerarPorEntidadAsync(codigo, idEntidad, pdf, ct);
                return pdf
                    ? File(bytes, "application/pdf", $"{codigo}_{idEntidad}.pdf")
                    : File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{codigo}_{idEntidad}.docx");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}/origen")]
        public async Task<ActionResult> ActualizarOrigen(int id, [FromBody] ActualizarOrigenRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _service.ActualizarOrigenAsync(id, request.Origen, ct));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}/roles")]
        public async Task<ActionResult> ActualizarRoles(int id, [FromBody] ActualizarRolesRequest request, CancellationToken ct)
        {
            try
            {
                return Ok(await _service.ActualizarRolesAsync(id, request.RolesGenera, ct));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult> Listar([FromQuery] string? categoria, CancellationToken ct)
        {
            return Ok(await _service.ListarAsync(categoria, ct));
        }

        [HttpGet("{codigo}/variables")]
        public ActionResult GetVariables(string codigo)
        {
            return Ok(_service.GetVariablesDisponibles(codigo));
        }

        [HttpPost]
        public async Task<ActionResult> Crear(
            [FromForm] string nombre,
            [FromForm] string codigo,
            [FromForm] string categoria,
            [FromForm] string? descripcion,
            [FromForm] string? variables,
            [FromForm] string? origen,
            [FromForm] string? rolesGenera,
            IFormFile archivo,
            CancellationToken ct)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { error = "Archivo .docx requerido" });

            var ext = Path.GetExtension(archivo.FileName).ToLower();
            if (ext != ".docx" && ext != ".xlsx" && ext != ".xls")
                return BadRequest(new { error = "Solo se aceptan archivos .docx, .xlsx o .xls" });

            try
            {
                var result = await _service.CrearAsync(nombre, codigo, categoria, descripcion, archivo.OpenReadStream(), archivo.FileName, variables ?? "[]", origen, rolesGenera, ct);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}/archivo")]
        public async Task<ActionResult> ActualizarArchivo(int id, IFormFile archivo, CancellationToken ct)
        {
            if (archivo == null) return BadRequest(new { error = "Archivo requerido" });
            var extUpd = Path.GetExtension(archivo.FileName).ToLower();
            if (extUpd != ".docx" && extUpd != ".xlsx" && extUpd != ".xls")
                return BadRequest(new { error = "Solo se aceptan archivos .docx, .xlsx o .xls" });

            try
            {
                var result = await _service.ActualizarArchivoAsync(id, archivo.OpenReadStream(), archivo.FileName, ct);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Eliminar(int id, CancellationToken ct)
        {
            try
            {
                await _service.EliminarAsync(id, ct);
                return Ok(new { mensaje = "Plantilla eliminada" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("generar/{codigo}")]
        public async Task<ActionResult> GenerarDocumento(string codigo, [FromBody] GenerarDocumentoRequest request, CancellationToken ct)
        {
            try
            {
                var docBytes = await _service.GenerarDocumentoAsync(codigo, request.Variables, request.Tablas, ct);
                return File(docBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{codigo}_{DateTime.Now:yyyyMMdd}.docx");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("generar/listado-grupo/{idGrupo:int}")]
        public async Task<ActionResult> GenerarListadoGrupo(int idGrupo, CancellationToken ct)
        {
            var plantilla = await _service.ObtenerPorCodigoAsync("listado_grupo", ct);
            if (plantilla == null)
                return NotFound(new { error = "Plantilla 'listado_grupo' no configurada. Suba una plantilla primero." });

            var grupo = await _db.Grupo
                .Include(g => g.IdPlanEstudiosNavigation)
                .Include(g => g.IdPeriodoAcademicoNavigation)
                .FirstOrDefaultAsync(g => g.IdGrupo == idGrupo, ct);

            if (grupo == null) return NotFound(new { error = "Grupo no encontrado" });

            var estudiantes = await _db.EstudianteGrupo
                .Include(eg => eg.IdEstudianteNavigation)
                    .ThenInclude(e => e.IdPersonaNavigation)
                .Where(eg => eg.IdGrupo == idGrupo && eg.Status == Core.Enums.StatusEnum.Active)
                .OrderBy(eg => eg.IdEstudianteNavigation.IdPersonaNavigation.ApellidoPaterno)
                .ToListAsync(ct);

            var variables = new Dictionary<string, string>
            {
                { "carrera", grupo.IdPlanEstudiosNavigation?.NombrePlanEstudios ?? "" },
                { "periodo", grupo.IdPeriodoAcademicoNavigation?.Nombre ?? "" },
                { "grupo", grupo.CodigoGrupo ?? grupo.NombreGrupo ?? "" }
            };

            var tablaEstudiantes = estudiantes.Select(eg =>
            {
                var p = eg.IdEstudianteNavigation?.IdPersonaNavigation;
                return new Dictionary<string, string>
                {
                    { "matricula", eg.IdEstudianteNavigation?.Matricula ?? "" },
                    { "estatus", eg.Estado ?? "Inscrito" },
                    { "nombre", p != null ? $"{p.ApellidoPaterno} {p.ApellidoMaterno} {p.Nombre}".Trim() : "" }
                };
            }).ToList();

            var tablas = new Dictionary<string, List<Dictionary<string, string>>>
            {
                { "tabla_estudiantes", tablaEstudiantes }
            };

            try
            {
                var docBytes = await _service.GenerarDocumentoAsync("listado_grupo", variables, tablas, ct);
                return File(docBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"Listado_Grupo_{grupo.CodigoGrupo}_{DateTime.Now:yyyyMMdd}.docx");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [HttpGet("preview/{codigo}")]
        public async Task<ActionResult> PreviewPlantilla(string codigo, CancellationToken ct)
        {
            var sampleVars = GetSampleData(codigo);
            var sampleTablas = GetSampleTables(codigo);

            try
            {
                var plantilla = await _service.ObtenerPorCodigoAsync(codigo, ct);
                var ext = plantilla != null ? System.IO.Path.GetExtension(plantilla.RutaArchivo) : ".docx";
                var docBytes = await _service.GenerarDocumentoAsync(codigo, sampleVars, sampleTablas, ct);
                var pdfBytes = await WebApplication2.Services.DocxToPdfConverter.ConvertAsync(docBytes, ext, ct);
                return File(pdfBytes, "application/pdf");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("preview-original/{codigo}")]
        public async Task<ActionResult> PreviewOriginal(string codigo, CancellationToken ct)
        {
            var plantilla = await _service.ObtenerPorCodigoAsync(codigo, ct);
            if (plantilla == null) return NotFound(new { error = "Plantilla no encontrada" });

            var bytes = await System.IO.File.ReadAllBytesAsync(plantilla.RutaArchivo, ct);
            var ext = System.IO.Path.GetExtension(plantilla.RutaArchivo);
            var pdfBytes = await WebApplication2.Services.DocxToPdfConverter.ConvertAsync(bytes, ext, ct);
            return File(pdfBytes, "application/pdf");
        }

        private Dictionary<string, string> GetSampleData(string codigo) => codigo switch
        {
            "listado_grupo" => new()
            {
                { "carrera", "Licenciatura en Enfermería" },
                { "periodo", "CUATRIMESTRE ENERO-ABRIL 2026" },
                { "grupo", "1A Matutino (111)" }
            },
            "lista_asistencia" => new()
            {
                { "carrera", "LICENCIATURA EN RADIOLOGÍA E IMAGEN" },
                { "periodo", "ENERO-ABRIL 2026" },
                { "materia", "Laboratorio De Simulación Imagenológica VII" },
                { "clave", "RALS0806" },
                { "clave_materia", "RALS0806" },
                { "docente", "José Laguna Zaragoza" },
                { "profesor", "José Laguna Zaragoza" },
                { "grupo", "831" },
                { "nombre_grupo", "8A Sabatino" },
                { "campus", "Campus Veracruz" },
                { "cuatrimestre", "8°" }
            },
            "constancia_estudios" => new()
            {
                { "nombre_alumno", "JUAN CARLOS PÉREZ LÓPEZ" },
                { "matricula", "L00001" },
                { "curp", "PELJ000101HGTRRN01" },
                { "carrera", "Licenciatura en Enfermería" },
                { "rvoe", "20231946" },
                { "periodo", "del 06 de enero al 30 de abril de 2026" },
                { "grado", "cursando el tercer cuatrimestre de diez" },
                { "campus", "León, Guanajuato" },
                { "fecha", "20 de marzo de 2026" },
                { "folio", "CONST-001-20260320" }
            },
            "kardex" => new()
            {
                { "nombre_alumno", "JUAN CARLOS PÉREZ LÓPEZ" },
                { "matricula", "L00001" },
                { "carrera", "LICENCIATURA EN ENFERMERÍA" },
                { "rvoe", "20231946" },
                { "ciclo_ingreso", "Enero 2025" }
            },
            "boleta_calificaciones" => new()
            {
                { "CARRERA", "LICENCIATURA EN RADIOLOGÍA E IMAGEN" },
                { "PERIODO", "ENERO-ABRIL 2026" },
                { "NOMBRE_ALUMNO", "JUAN CARLOS PÉREZ LÓPEZ" },
                { "MATRICULA", "L00001" },
                { "GRUPO", "8A Sabatino" }
            },
            _ => new()
        };

        private Dictionary<string, List<Dictionary<string, string>>>? GetSampleTables(string codigo) => codigo switch
        {
            "listado_grupo" => new()
            {
                { "tabla_estudiantes", new List<Dictionary<string, string>>
                    {
                        new() { { "matricula", "L00501" }, { "estatus", "Inscrito" }, { "nombre", "ALCARÁZ GAYTÁN ÁNGEL DANIEL" } },
                        new() { { "matricula", "L00502" }, { "estatus", "Inscrito" }, { "nombre", "GARCÍA HERNÁNDEZ MARÍA FERNANDA" } },
                        new() { { "matricula", "L00503" }, { "estatus", "Inscrito" }, { "nombre", "LÓPEZ MARTÍNEZ JOSÉ ANTONIO" } },
                        new() { { "matricula", "L00504" }, { "estatus", "Inscrito" }, { "nombre", "RAMÍREZ SILVA KAREN LIZBETH" } },
                        new() { { "matricula", "L00505" }, { "estatus", "Inscrito" }, { "nombre", "TORRES GARCÍA HÉCTOR GAEL" } },
                    }
                }
            },
            "boleta_calificaciones" => new()
            {
                { "tabla_materias", new List<Dictionary<string, string>>
                    {
                        new() { { "clave", "ENFE101" }, { "nombre_materia", "Anatomía Humana I" }, { "calificacion", "8.5" } },
                        new() { { "clave", "ENFE102" }, { "nombre_materia", "Enfermería y Salud" }, { "calificacion", "9.0" } },
                        new() { { "clave", "ENFE103" }, { "nombre_materia", "Psicología" }, { "calificacion", "7.8" } },
                        new() { { "clave", "ENFE104" }, { "nombre_materia", "Bioquímica" }, { "calificacion", "8.2" } },
                        new() { { "clave", "ENFE105" }, { "nombre_materia", "Inglés Técnico I" }, { "calificacion", "9.5" } },
                    }
                }
            },
            "lista_asistencia" => new()
            {
                { "tabla_estudiantes", new List<Dictionary<string, string>>
                    {
                        new() { { "matricula", "L00029" }, { "estatus", "Inscrito" }, { "nombre", "ALANIS SALDAÑA ALVARO" } },
                        new() { { "matricula", "L00030" }, { "estatus", "Inscrito" }, { "nombre", "ARAUJO MARTINEZ ERIK ULISES" } },
                        new() { { "matricula", "L00032" }, { "estatus", "Inscrito" }, { "nombre", "CAUDILLO GARNICA JOAQUIN" } },
                        new() { { "matricula", "L00033" }, { "estatus", "Inscrito" }, { "nombre", "CHAVEZ SANCHEZ MAYRA BEATRIZ" } },
                        new() { { "matricula", "L00099" }, { "estatus", "Inscrito" }, { "nombre", "GONZÁLEZ REYES KAREN ALEJANDRA" } },
                        new() { { "matricula", "L00035" }, { "estatus", "Inscrito" }, { "nombre", "LOPEZ CRUZ JESUS" } },
                        new() { { "matricula", "L00037" }, { "estatus", "Inscrito" }, { "nombre", "MEDEL RAMIREZ MIGUEL" } },
                    }
                }
            },
            "kardex" => new()
            {
                { "tabla_materias", new List<Dictionary<string, string>>
                    {
                        new() { { "materia", "Anatomía Humana I" }, { "clave", "ENFE101" }, { "ciclo", "ENE-ABR 2025" }, { "calificacion", "8.5" }, { "promedio", "8.5" } },
                        new() { { "materia", "Enfermería y Salud" }, { "clave", "ENFE102" }, { "ciclo", "ENE-ABR 2025" }, { "calificacion", "9.0" }, { "promedio", "9.0" } },
                        new() { { "materia", "Psicología" }, { "clave", "ENFE103" }, { "ciclo", "ENE-ABR 2025" }, { "calificacion", "7.8" }, { "promedio", "7.8" } },
                    }
                }
            },
            _ => null
        };
    }

    public class GenerarDocumentoRequest
    {
        public Dictionary<string, string> Variables { get; set; } = new();
        public Dictionary<string, List<Dictionary<string, string>>>? Tablas { get; set; }
    }
}
