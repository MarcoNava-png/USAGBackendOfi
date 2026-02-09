using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.ConceptoPago
{
    public class ConceptoPagoCreateRequest
    {
        public string Clave { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string? Tipo { get; set; }
        public int AplicaA { get; set; }
        public bool EsObligatorio { get; set; } = true;
        public int PeriodicidadMeses { get; set; } = 0;
        public bool Activo { get; set; } = true;
    }
}
