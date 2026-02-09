using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class ActualizarConceptoDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Tipo { get; set; }  
        public bool? PermiteBeca { get; set; }

        public ConceptoTipoEnum? ConceptoTipo { get; set; }
        public ConceptoAplicaAEnum? ConceptoAplica { get; set; }
        public bool? EsObligatorio { get; set; }
        public byte? PeriodicidadMeses { get; set; }
    }
}
