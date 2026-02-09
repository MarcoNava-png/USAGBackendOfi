using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Configuration.Constants;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Controllers;

[ApiController]
[Route("api/reportes-academicos")]
[Authorize(Roles = $"{Rol.ADMIN},{Rol.DIRECTOR},{Rol.COORDINADOR},{Rol.CONTROL_ESCOLAR},{Rol.DOCENTE}")]
public class ReporteAcademicoController : ControllerBase
{
    private readonly IReporteAcademicoService _svc;

    public ReporteAcademicoController(IReporteAcademicoService svc) => _svc = svc;

    // ──────── Estudiantes por Grupo ────────

    [HttpGet("estudiantes-grupo/{idGrupo:int}")]
    public async Task<IActionResult> GetEstudiantesPorGrupo(int idGrupo, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetEstudiantesPorGrupoAsync(idGrupo, ct);
            return Ok(data);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("estudiantes-grupo/{idGrupo:int}/pdf")]
    public async Task<IActionResult> GetEstudiantesPorGrupoPdf(int idGrupo, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetEstudiantesPorGrupoAsync(idGrupo, ct);
            var pdf = _svc.GenerarEstudiantesPorGrupoPdf(data);
            return File(pdf, "application/pdf", $"Estudiantes_{data.NombreGrupo}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("estudiantes-grupo/{idGrupo:int}/excel")]
    public async Task<IActionResult> GetEstudiantesPorGrupoExcel(int idGrupo, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetEstudiantesPorGrupoAsync(idGrupo, ct);
            var excel = _svc.GenerarEstudiantesPorGrupoExcel(data);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Estudiantes_{data.NombreGrupo}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ──────── Boleta de Calificaciones ────────

    [HttpGet("boleta/{idEstudiante:int}/{idPeriodo:int}/pdf")]
    public async Task<IActionResult> GetBoletaPdf(int idEstudiante, int idPeriodo, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetBoletaCalificacionesAsync(idEstudiante, idPeriodo, ct);
            var pdf = _svc.GenerarBoletaCalificacionesPdf(data);
            return File(pdf, "application/pdf", $"Boleta_{data.Matricula}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ──────── Acta de Calificación ────────

    [HttpGet("acta/{idGrupoMateria:int}/pdf")]
    public async Task<IActionResult> GetActaPdf(int idGrupoMateria, [FromQuery] int? parcialId, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetActaCalificacionAsync(idGrupoMateria, parcialId, ct);
            var pdf = _svc.GenerarActaCalificacionPdf(data);
            return File(pdf, "application/pdf", $"Acta_{data.ClaveMateria}_{data.NombreParcial}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ──────── Horario de Grupo ────────

    [HttpGet("horario-grupo/{idGrupo:int}/pdf")]
    public async Task<IActionResult> GetHorarioGrupoPdf(int idGrupo, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetHorarioGrupoAsync(idGrupo, ct);
            var pdf = _svc.GenerarHorarioPdf(data);
            return File(pdf, "application/pdf", $"Horario_{data.Titulo.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("horario-grupo/{idGrupo:int}/excel")]
    public async Task<IActionResult> GetHorarioGrupoExcel(int idGrupo, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetHorarioGrupoAsync(idGrupo, ct);
            var excel = _svc.GenerarHorarioExcel(data);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Horario_{data.Titulo.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ──────── Horario de Docente ────────

    [HttpGet("horario-docente/{idProfesor:int}/{idPeriodo:int}/pdf")]
    public async Task<IActionResult> GetHorarioDocentePdf(int idProfesor, int idPeriodo, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetHorarioDocenteAsync(idProfesor, idPeriodo, ct);
            var pdf = _svc.GenerarHorarioPdf(data);
            return File(pdf, "application/pdf", $"Horario_Docente_{idProfesor}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("horario-docente/{idProfesor:int}/{idPeriodo:int}/excel")]
    public async Task<IActionResult> GetHorarioDocenteExcel(int idProfesor, int idPeriodo, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetHorarioDocenteAsync(idProfesor, idPeriodo, ct);
            var excel = _svc.GenerarHorarioExcel(data);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Horario_Docente_{idProfesor}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ──────── Lista de Asistencia ────────

    [HttpGet("lista-asistencia/{idGrupoMateria:int}/pdf")]
    public async Task<IActionResult> GetListaAsistenciaPdf(int idGrupoMateria, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetListaAsistenciaAsync(idGrupoMateria, ct);
            var pdf = _svc.GenerarListaAsistenciaPdf(data);
            return File(pdf, "application/pdf", $"ListaAsistencia_{data.NombreMateria}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ──────── Planes de Estudio ────────

    [HttpGet("planes-estudio/excel")]
    public async Task<IActionResult> GetPlanesEstudioExcel(CancellationToken ct)
    {
        try
        {
            var excel = await _svc.GenerarPlanesEstudioExcelAsync(ct);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"PlanesEstudio_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
