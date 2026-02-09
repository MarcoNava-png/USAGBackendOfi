using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs.Recibo
{
    public class ReciboBusquedaFiltrosDto
    {
        public string? Folio { get; set; }
        public string? Matricula { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public EstatusRecibo? Estatus { get; set; }
        public bool SoloVencidos { get; set; } = false;
        public bool SoloPagados { get; set; } = false;
        public bool SoloPendientes { get; set; } = false;
        public DateOnly? FechaEmisionDesde { get; set; }
        public DateOnly? FechaEmisionHasta { get; set; }
        public DateOnly? FechaVencimientoDesde { get; set; }
        public DateOnly? FechaVencimientoHasta { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanioPagina { get; set; } = 50;
    }
}
