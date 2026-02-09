using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.Pagos
{
    public class PlanDetalleCreateRequest
    {
        public int IdPlanPago { get; set; }
        public int IdConceptoPago { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Importe { get; set; }                        
        public bool EsInscripcion { get; set; }
        public bool EsMensualidad { get; set; }
        public int MesOffset { get; set; }
        public byte? DiaPago { get; set; }
        public int Orden { get; set; }
    }
}
