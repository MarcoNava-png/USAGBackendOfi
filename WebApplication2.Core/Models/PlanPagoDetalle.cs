using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Models
{
    public class PlanPagoDetalle : BaseEntity
    {
        public int IdPlanPagoDetalle { get; set; }
        public int IdPlanPago { get; set; }
        public int Orden { get; set; }
        public int IdConceptoPago { get; set; }
        public string Descripcion { get; set; } = null!;
        public decimal Cantidad { get; set; } = 1m;
        public decimal Importe { get; set; }         
        public bool EsInscripcion { get; set; }
        public bool EsMensualidad { get; set; }
        public int MesOffset { get; set; } = 0;
        public byte? DiaPago { get; set; }
        public bool PintaInternet { get; set; } = true;

        public PlanPago PlanPago { get; set; } = null!;
        public ConceptoPago ConceptoPago { get; set; } = null!;
    }
}
