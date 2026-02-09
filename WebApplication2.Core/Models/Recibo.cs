using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public class Recibo : BaseEntity
    {
        public long IdRecibo { get; set; }
        public string? Folio { get; set; }
        public int? IdAspirante { get; set; }
        public int? IdEstudiante { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVencimiento { get; set; }
        public EstatusRecibo Estatus { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargos { get; set; }
        public decimal Total { get; private set; }
        public decimal Saldo { get; set; }
        public string? Notas { get; set; }

        public ICollection<ReciboDetalle> Detalles { get; set; } = new List<ReciboDetalle>();
        public ICollection<LigaPago> Ligas { get; set; } = new List<LigaPago>();
        public ICollection<BitacoraRecibo> Bitacora { get; set; } = new List<BitacoraRecibo>();
    }
}
