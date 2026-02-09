using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Models
{
    public class ReciboDetalle : BaseEntity
    {
        public long IdReciboDetalle { get; set; }
        public long IdRecibo { get; set; }
        public int IdConceptoPago { get; set; }
        public string Descripcion { get; set; } = null!;
        public decimal Cantidad { get; set; } = 1m;
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; private set; }
        public string? RefTabla { get; set; }
        public long? RefId { get; set; }

        public Recibo Recibo { get; set; } = null!;
        public ConceptoPago ConceptoPago { get; set; } = null!;
        public ICollection<PagoAplicacion> Aplicaciones { get; set; } = new List<PagoAplicacion>();
    }
}
