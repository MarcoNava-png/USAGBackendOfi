using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public class Pago : BaseEntity
    {
        public long IdPago { get; set; }
        public string? FolioPago { get; set; }
        public DateTime FechaPagoUtc { get; set; } = DateTime.UtcNow;
        public int IdMedioPago { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
        public EstatusPago Estatus { get; set; } = EstatusPago.CONFIRMADO;
        public string? IdUsuarioCaja { get; set; }
        public int? IdCaja { get; set; }
        public int? IdCorteCaja { get; set; }

        public MedioPago MedioPago { get; set; } = null!;
        public ICollection<PagoAplicacion> Aplicaciones { get; set; } = new List<PagoAplicacion>();
    }
}
