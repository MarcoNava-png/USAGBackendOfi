namespace WebApplication2.Core.DTOs.Aspirante;

public class InscripcionPreviaAspiranteDto
{
    public int IdAspirante { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string MatriculaProyectada { get; set; } = string.Empty;
    public string CorreoProyectado { get; set; } = string.Empty;
    public int? IdPlanEstudios { get; set; }
    public string? NombrePlanEstudios { get; set; }
    public string? ClavePlanEstudios { get; set; }
    public string? Campus { get; set; }
    public int? IdPeriodoAcademico { get; set; }
    public string? NombrePeriodoAcademico { get; set; }
    public string? TurnoAspirante { get; set; }
    public int? IdTurnoAspirante { get; set; }
    public List<GrupoDisponibleParaAspiranteDto> GruposDisponibles { get; set; } = new();
}
