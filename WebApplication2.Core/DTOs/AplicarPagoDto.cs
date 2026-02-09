using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class AplicarPagoDto
    {
        public long IdPago { get; set; }
        public List<AplicacionLineaDto> Aplicaciones { get; set; } = new();
    }
}
