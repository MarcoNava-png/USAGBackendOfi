using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Requests.ConceptoPrecio;

namespace WebApplication2.Core.Requests.ConceptoPagoConPrecio
{
    public class ConceptoPagoConPrecioCreateRequest
    {
        public string Clave { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? Tipo { get; set; }
        public int AplicaA { get; set; }
        public bool EsObligatorio { get; set; } = true;
        public int PeriodicidadMeses { get; set; } = 0;
        public bool Activo { get; set; } = true;
        public ConceptoPrecioCreateRequest Precio { get; set; } = null!;
    }
}
