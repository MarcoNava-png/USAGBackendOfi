using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Requests.Pagos
{
    public class GenerarRecibosRequest
    {
        public int IdEstudiante { get; set; }
        public int IdPeriodoAcademico { get; set; }
        public int? IdPlanPago { get; set; }
        public ConceptoTipoEnum tipo { get; set; }      
        public byte DiaVencimiento { get; set; }           
        public bool AplicarBecas { get; set; } = true;
    }
}
