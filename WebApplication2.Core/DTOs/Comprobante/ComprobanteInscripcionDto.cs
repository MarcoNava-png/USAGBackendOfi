namespace WebApplication2.Core.DTOs.Comprobante;

public class ComprobanteInscripcionDto
{
    public int IdEstudiante { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Curp { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public string PlanEstudios { get; set; } = string.Empty;
    public string? ClavePlanEstudios { get; set; }
    public string? Campus { get; set; }
    public string? Turno { get; set; }
    public string? GrupoCodigo { get; set; }
    public string? GrupoNombre { get; set; }
    public int? NumeroCuatrimestre { get; set; }
    public string? PeriodoAcademico { get; set; }
    public string CorreoInstitucional { get; set; } = string.Empty;
    public string? PasswordTemporal { get; set; }
    public string? UrlPortal { get; set; }
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    public bool IncluyeCredenciales { get; set; }
}
