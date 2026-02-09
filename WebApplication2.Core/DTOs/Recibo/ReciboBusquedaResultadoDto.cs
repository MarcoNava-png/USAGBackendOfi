namespace WebApplication2.Core.DTOs.Recibo
{
    public class ReciboBusquedaResultadoDto
    {
        public List<ReciboExtendidoDto> Recibos { get; set; } = new List<ReciboExtendidoDto>();
        public int TotalRegistros { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TamanioPagina { get; set; }
        public decimal TotalSaldoPendiente { get; set; }
        public decimal TotalRecargos { get; set; }
        public int TotalVencidos { get; set; }
        public int TotalPagados { get; set; }
        public int TotalPendientes { get; set; }
    }
}
