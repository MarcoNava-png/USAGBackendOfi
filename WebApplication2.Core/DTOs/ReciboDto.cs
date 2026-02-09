using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class ReciboDto
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public int? IdAspirante { get; set; }
        public int? IdEstudiante { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public EstatusRecibo estatus { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargos { get; set; }
        public decimal Total { get; set; }
        public decimal Saldo { get; set; }
        public string? Notas { get; set; }
        public List<ReciboLineaDto> Detalles { get; set; } = new List<ReciboLineaDto>();
    }
}
