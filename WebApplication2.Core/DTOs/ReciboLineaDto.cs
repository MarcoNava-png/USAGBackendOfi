using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class ReciboLineaDto
    {
        public long IdReciboDetalle { get; set; }
        public int IdConceptoPago { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
        public string? RefTabla { get; set; }
        public long? RefId { get; set; }
    }
}
