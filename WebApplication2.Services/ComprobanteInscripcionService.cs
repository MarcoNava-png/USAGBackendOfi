using Microsoft.EntityFrameworkCore;
using WebApplication2.Core.DTOs.Comprobante;
using WebApplication2.Core.Enums;
using WebApplication2.Data.DbContexts;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services;

public class ComprobanteInscripcionService : IComprobanteInscripcionService
{
    private readonly ApplicationDbContext _db;
    private readonly IPdfService _pdfService;

    public ComprobanteInscripcionService(ApplicationDbContext db, IPdfService pdfService)
    {
        _db = db;
        _pdfService = pdfService;
    }

    public async Task<ComprobanteInscripcionDto?> ObtenerAsync(int idEstudiante, string? passwordTemporal = null, CancellationToken ct = default)
    {
        var estudiante = await _db.Estudiante
            .AsNoTracking()
            .Include(e => e.IdPersonaNavigation)
            .Include(e => e.IdPlanActualNavigation)
                .ThenInclude(p => p!.IdCampusNavigation)
            .FirstOrDefaultAsync(e => e.IdEstudiante == idEstudiante, ct);

        if (estudiante == null) return null;
        var persona = estudiante.IdPersonaNavigation;
        if (persona == null) return null;

        var grupoActual = await _db.EstudianteGrupo
            .AsNoTracking()
            .Where(eg => eg.IdEstudiante == idEstudiante && eg.Status == StatusEnum.Active)
            .Include(eg => eg.IdGrupoNavigation!)
                .ThenInclude(g => g.IdPeriodoAcademicoNavigation)
            .Include(eg => eg.IdGrupoNavigation!)
                .ThenInclude(g => g.IdTurnoNavigation)
            .OrderByDescending(eg => eg.FechaInscripcion)
            .Select(eg => new
            {
                eg.IdGrupoNavigation!.CodigoGrupo,
                eg.IdGrupoNavigation.NombreGrupo,
                Cuatri = (int)eg.IdGrupoNavigation.NumeroCuatrimestre,
                Periodo = eg.IdGrupoNavigation.IdPeriodoAcademicoNavigation != null ? eg.IdGrupoNavigation.IdPeriodoAcademicoNavigation.Nombre : null,
                Turno = eg.IdGrupoNavigation.IdTurnoNavigation != null ? eg.IdGrupoNavigation.IdTurnoNavigation.Nombre : null
            })
            .FirstOrDefaultAsync(ct);

        return new ComprobanteInscripcionDto
        {
            IdEstudiante = estudiante.IdEstudiante,
            Matricula = estudiante.Matricula,
            NombreCompleto = $"{persona.Nombre} {persona.ApellidoPaterno} {persona.ApellidoMaterno}".Trim(),
            Curp = persona.Curp,
            FechaIngreso = estudiante.FechaIngreso,
            PlanEstudios = estudiante.IdPlanActualNavigation?.NombrePlanEstudios ?? "N/A",
            ClavePlanEstudios = estudiante.IdPlanActualNavigation?.ClavePlanEstudios,
            Campus = estudiante.IdPlanActualNavigation?.IdCampusNavigation?.Nombre,
            Turno = grupoActual?.Turno,
            GrupoCodigo = grupoActual?.CodigoGrupo,
            GrupoNombre = grupoActual?.NombreGrupo,
            NumeroCuatrimestre = grupoActual?.Cuatri,
            PeriodoAcademico = grupoActual?.Periodo,
            CorreoInstitucional = estudiante.Email ?? $"{estudiante.Matricula}@usaguanajuato.edu.mx",
            PasswordTemporal = passwordTemporal,
            IncluyeCredenciales = !string.IsNullOrEmpty(passwordTemporal),
            UrlPortal = "https://saciusag.com.mx",
            FechaGeneracion = DateTime.UtcNow
        };
    }

    public async Task<byte[]> GenerarPdfAsync(int idEstudiante, string? passwordTemporal = null, CancellationToken ct = default)
    {
        var dto = await ObtenerAsync(idEstudiante, passwordTemporal, ct)
            ?? throw new KeyNotFoundException($"Estudiante {idEstudiante} no encontrado");
        return _pdfService.GenerarComprobanteInscripcion(dto);
    }
}
