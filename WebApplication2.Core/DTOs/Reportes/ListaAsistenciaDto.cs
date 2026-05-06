namespace WebApplication2.Core.DTOs.Reportes;

public class ListaAsistenciaDto
{
    public string NombreGrupo { get; set; } = null!;
    public string? CodigoGrupo { get; set; }
    public string NombreMateria { get; set; } = null!;
    public string? ClaveMateria { get; set; }
    public string? NombreProfesor { get; set; }
    public string PeriodoAcademico { get; set; } = null!;
    public string? PlanEstudios { get; set; }
    public string? Campus { get; set; }
    public int? Cuatrimestre { get; set; }
    public List<AlumnoListaDto> Alumnos { get; set; } = [];
}

public class AlumnoListaDto
{
    public string Matricula { get; set; } = null!;
    public string NombreCompleto { get; set; } = null!;
    public string Estatus { get; set; } = "Inscrito";
}
