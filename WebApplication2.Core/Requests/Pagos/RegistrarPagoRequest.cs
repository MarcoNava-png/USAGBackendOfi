using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.Pagos
{
    public class RegistrarPagoRequest
    {
        public DateTime FechaPagoUtc { get; set; }
        public int IdMedioPago { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; }                         
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
        public EstatusPago estatus { get; set; } = EstatusPago.CONFIRMADO;
    }
}
