namespace WebApplication2.Core.DTOs.Reportes;

public class HorarioReporteDto
{
    public string Titulo { get; set; } = null!;
    public string? Subtitulo { get; set; }
    public List<BloqueHorarioDto> Bloques { get; set; } = [];
}

public class BloqueHorarioDto
{
    public string DiaSemana { get; set; } = null!;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public string NombreMateria { get; set; } = null!;
    public string? Profesor { get; set; }
    public string? Grupo { get; set; }
    public string? Aula { get; set; }
}
