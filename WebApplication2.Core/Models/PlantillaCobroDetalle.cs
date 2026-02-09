using System;

namespace WebApplication2.Core.Models
{
    public class PlantillaCobroDetalle
    {
        public int IdPlantillaDetalle { get; set; }

        public int IdPlantillaCobro { get; set; }

        public int IdConceptoPago { get; set; }

        public string Descripcion { get; set; } = null!;

        public decimal Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public int Orden { get; set; }

        public int? AplicaEnRecibo { get; set; }

        public virtual PlantillaCobro? IdPlantillaCobroNavigation { get; set; }
        public virtual ConceptoPago? IdConceptoPagoNavigation { get; set; }
    }
}
