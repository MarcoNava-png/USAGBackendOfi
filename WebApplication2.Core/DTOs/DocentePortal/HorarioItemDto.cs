namespace WebApplication2.Core.DTOs.DocentePortal;

public class HorarioItemDto
{
    public string Dia { get; set; } = null!;
    public string HoraInicio { get; set; } = null!;
    public string HoraFin { get; set; } = null!;
    public string? Aula { get; set; }
}
