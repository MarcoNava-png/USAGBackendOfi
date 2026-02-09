using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Models
{
    public class PagoAplicacion : BaseEntity
    {
        public long IdPagoAplicacion { get; set; }
        public long IdPago { get; set; }
        public long IdReciboDetalle { get; set; }
        public decimal MontoAplicado { get; set; }

        public Pago Pago { get; set; } = null!;
        public ReciboDetalle ReciboDetalle { get; set; } = null!;
    }
}
