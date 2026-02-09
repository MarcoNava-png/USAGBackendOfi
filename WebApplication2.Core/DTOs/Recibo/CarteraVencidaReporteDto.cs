namespace WebApplication2.Core.DTOs.Recibo
{
    public class CarteraVencidaReporteDto
    {
        public DateOnly FechaReporte { get; set; }
        public string? NombrePeriodo { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public int TotalRecibosVencidos { get; set; }
        public decimal TotalSaldoVencido { get; set; }
        public decimal TotalRecargos { get; set; }
        public decimal TotalAdeudo { get; set; }
        public List<CarteraVencidaItemDto> Detalle { get; set; } = new List<CarteraVencidaItemDto>();
    }
}
