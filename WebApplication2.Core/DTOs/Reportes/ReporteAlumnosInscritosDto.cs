namespace WebApplication2.Core.DTOs.Reportes;

public class ReporteAlumnosInscritosDto
{
    public List<int> IdsPeriodo { get; set; } = new();
    public List<string> NombresPeriodo { get; set; } = new();
    public int? IdPlanEstudios { get; set; }
    public string? NombrePlanEstudios { get; set; }
    public int? IdCampus { get; set; }
    public string? NombreCampus { get; set; }
    public int TotalAlumnos { get; set; }
    public List<AlumnoInscritoReporteDto> Alumnos { get; set; } = new();
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
}

public class AlumnoInscritoReporteDto
{
    public int IdEstudiante { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string PlanEstudios { get; set; } = string.Empty;
    public string? ClavePlan { get; set; }
    public string? Campus { get; set; }
    public string? GrupoCodigo { get; set; }
    public string? Turno { get; set; }
    public int? NumeroCuatrimestre { get; set; }
    public string PeriodoAcademico { get; set; } = string.Empty;
    public DateTime? FechaInscripcion { get; set; }
    public string Estado { get; set; } = string.Empty;
}
