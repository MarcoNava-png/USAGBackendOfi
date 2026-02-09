using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.Pagos
{
    public class AplicarPagoRequest
    {
        public long IdPago { get; set; }
        public List<AplicacionLineaRequest> Aplicaciones { get; set; } = new();
    }
}
