namespace WebApplication2.Core.DTOs.Reportes;

public class ActaCalificacionDto
{
    public string NombreGrupo { get; set; } = null!;
    public string NombreMateria { get; set; } = null!;
    public string ClaveMateria { get; set; } = null!;
    public string? NombreProfesor { get; set; }
    public string PeriodoAcademico { get; set; } = null!;
    public string? NombreParcial { get; set; }
    public List<AlumnoActaDto> Alumnos { get; set; } = [];
}

public class AlumnoActaDto
{
    public string Matricula { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public decimal? Calificacion { get; set; }
    public string? Estado { get; set; }
}
