using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public class ConceptoPago : BaseEntity
    {
        public int IdConceptoPago { get; set; }
        public string Clave { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public ConceptoTipoEnum Tipo { get; set; }
        public ConceptoAplicaAEnum AplicaA { get; set; }
        public bool EsObligatorio { get; set; }
        public byte? PeriodicidadMeses { get; set; }
        public bool PermiteBeca { get; set; } = true;
        public bool Activo { get; set; } = true;

        public ICollection<ConceptoPrecio> Precios { get; set; } = new List<ConceptoPrecio>();
    }
}
