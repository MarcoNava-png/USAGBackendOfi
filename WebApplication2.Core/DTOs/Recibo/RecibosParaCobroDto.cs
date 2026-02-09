namespace WebApplication2.Core.DTOs.Recibo
{
    public class RecibosParaCobroDto
    {
        public EstudianteInfoDto? Estudiante { get; set; }
        public List<ReciboParaCobroDto> Recibos { get; set; } = new();
        public decimal TotalAdeudo { get; set; }
        public decimal TotalPagado { get; set; }
        public bool Multiple { get; set; }
        public List<EstudianteInfoDto>? Estudiantes { get; set; }
    }
}
