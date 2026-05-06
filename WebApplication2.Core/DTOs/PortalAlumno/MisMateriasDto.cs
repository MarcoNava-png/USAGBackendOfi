namespace WebApplication2.Core.DTOs.PortalAlumno;

public class MisMateriasDto
{
    public int TotalMaterias { get; set; }
    public int MateriasConCalificacionFinal { get; set; }
    public int MateriasEnCurso { get; set; }
    public string? PeriodoAcademico { get; set; }
    public List<MiMateriaInscritaDto> Materias { get; set; } = new();
}

public class MiMateriaInscritaDto
{
    public int IdInscripcion { get; set; }
    public int IdGrupoMateria { get; set; }
    public int IdMateria { get; set; }
    public string ClaveMateria { get; set; } = string.Empty;
    public string NombreMateria { get; set; } = string.Empty;
    public decimal Creditos { get; set; }
    public string GrupoCodigo { get; set; } = string.Empty;
    public byte? NumeroCuatrimestre { get; set; }
    public string? Aula { get; set; }
    public string? Docente { get; set; }
    public DateTime FechaInscripcion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal? CalificacionFinal { get; set; }
    public List<MiHorarioClaseDto> Horarios { get; set; } = new();
}

public class MiHorarioClaseDto
{
    public int IdHorario { get; set; }
    public byte IdDiaSemana { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public string? Aula { get; set; }
}
