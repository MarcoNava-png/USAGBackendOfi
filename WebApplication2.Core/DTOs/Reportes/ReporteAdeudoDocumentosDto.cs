namespace WebApplication2.Core.DTOs.Reportes;

public class ReporteAdeudoDocumentosDto
{
    public List<int> IdsPlanEstudios { get; set; } = new();
    public List<string> NombresPlanEstudios { get; set; } = new();
    public int? IdPeriodoAcademico { get; set; }
    public string? NombrePeriodoAcademico { get; set; }
    public int TotalAlumnosConAdeudo { get; set; }
    public int TotalDocumentosFaltantes { get; set; }
    public int ConProrrogaVigente { get; set; }
    public int ConProrrogaVencida { get; set; }
    public int SinProrroga { get; set; }
    public List<AlumnoAdeudoDocumentoDto> Alumnos { get; set; } = new();
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
}

public class AlumnoAdeudoDocumentoDto
{
    public int IdEstudiante { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string PlanEstudios { get; set; } = string.Empty;
    public string? GrupoCodigo { get; set; }
    public int DocumentosFaltantes { get; set; }
    public int ConProrrogaVigente { get; set; }
    public int ConProrrogaVencida { get; set; }
    public int SinProrroga { get; set; }
    public List<DocumentoFaltanteDto> Detalle { get; set; } = new();
}

public class DocumentoFaltanteDto
{
    public string Clave { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? FechaProrroga { get; set; }
    public string? MotivoProrroga { get; set; }
    public string EstatusProrroga { get; set; } = string.Empty;
}
