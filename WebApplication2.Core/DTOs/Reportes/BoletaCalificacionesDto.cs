namespace WebApplication2.Core.DTOs.Reportes;

public class BoletaCalificacionesDto
{
    public string Matricula { get; set; } = null!;
    public string NombreEstudiante { get; set; } = null!;
    public string PlanEstudios { get; set; } = null!;
    public string PeriodoAcademico { get; set; } = null!;
    public string? Campus { get; set; }
    public List<MateriaBoletaDto> Materias { get; set; } = [];
    public decimal PromedioGeneral { get; set; }
}

public class MateriaBoletaDto
{
    public string ClaveMateria { get; set; } = null!;
    public string NombreMateria { get; set; } = null!;
    public int Creditos { get; set; }
    public decimal? P1 { get; set; }
    public decimal? P2 { get; set; }
    public decimal? P3 { get; set; }
    public decimal? CalificacionFinal { get; set; }
    public string? Estado { get; set; }
}
