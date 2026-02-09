namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class HorarioItemDto
    {
        public string Dia { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public string HoraFin { get; set; } = string.Empty;
        public string? Aula { get; set; }
    }
}
