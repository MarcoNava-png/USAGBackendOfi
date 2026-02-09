namespace WebApplication2.Core.DTOs.Reportes;

public class ListaAsistenciaDto
{
    public string NombreGrupo { get; set; } = null!;
    public string NombreMateria { get; set; } = null!;
    public string? NombreProfesor { get; set; }
    public string PeriodoAcademico { get; set; } = null!;
    public List<AlumnoListaDto> Alumnos { get; set; } = [];
}

public class AlumnoListaDto
{
    public string Matricula { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
}
