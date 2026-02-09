using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class ConceptoDto
    {
        public int IdConceptoPago { get; set; }
        public string Clave { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string Tipo { get; set; } = null!;
        public bool PermiteBeca { get; set; }
        public int Status { get; set; }  
        public ConceptoTipoEnum? ConceptoTipo { get; set; }
        public ConceptoAplicaAEnum? ConceptoAplica { get; set; }
        public bool EsObligatorio { get; set; }
        public byte? PeriodicidadMeses { get; set; }
    }
}
