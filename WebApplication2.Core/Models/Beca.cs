using System;
using System.Collections.Generic;

namespace WebApplication2.Core.Models
{
    public class Beca : BaseEntity
    {
        public int IdBeca { get; set; }

        public string Clave { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        public string Tipo { get; set; } = "PORCENTAJE";

        public decimal Valor { get; set; }

        public decimal? TopeMensual { get; set; }

        public int? IdConceptoPago { get; set; }

        public bool Activo { get; set; } = true;

        public virtual ConceptoPago? ConceptoPago { get; set; }

        public virtual ICollection<BecaAsignacion> Asignaciones { get; set; } = new List<BecaAsignacion>();
    }
}
