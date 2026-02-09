namespace WebApplication2.Core.DTOs.Reportes;

public class ReporteEstudiantesGrupoDto
{
    public string NombreGrupo { get; set; } = null!;
    public string PlanEstudios { get; set; } = null!;
    public string PeriodoAcademico { get; set; } = null!;
    public string Turno { get; set; } = null!;
    public int TotalEstudiantes { get; set; }
    public List<EstudianteGrupoItemDto> Estudiantes { get; set; } = [];
}

public class EstudianteGrupoItemDto
{
    public int IdEstudiante { get; set; }
    public string Matricula { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string Estado { get; set; } = null!;
}
