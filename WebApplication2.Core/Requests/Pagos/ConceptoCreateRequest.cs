using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.Pagos
{
    public class ConceptoCreateRequest
    {
        public string Clave { get; set; }
        public string Descripcion { get; set; }
        public ConceptoTipoEnum conceptoTipo { get; set; }          
        public ConceptoAplicaAEnum conceptoAplica { get; set; }      
        public bool EsObligatorio { get; set; }
        public byte? PeriodicidadMeses { get; set; }
    }
}
