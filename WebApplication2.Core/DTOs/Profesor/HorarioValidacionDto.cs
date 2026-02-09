namespace WebApplication2.Core.DTOs.Profesor
{
    public class HorarioValidacionDto
    {
        public string Dia { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
        public string? Aula { get; set; }
    }
}
