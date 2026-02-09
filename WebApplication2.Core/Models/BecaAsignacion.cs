using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Models
{
    public class BecaAsignacion
    {
        public long IdBecaAsignacion { get; set; }

        public int IdEstudiante { get; set; }

        public int? IdBeca { get; set; }

        public int? IdConceptoPago { get; set; }

        public string Tipo { get; set; } = "PORCENTAJE";

        public decimal Valor { get; set; }

        public decimal? TopeMensual { get; set; }

        public int? IdPeriodoAcademico { get; set; }

        public DateOnly VigenciaDesde { get; set; }
        public DateOnly? VigenciaHasta { get; set; }
        public bool Activo { get; set; } = true;
        public string? Observaciones { get; set; }

        public virtual Estudiante? Estudiante { get; set; }
        public virtual Beca? Beca { get; set; }
        public virtual ConceptoPago? ConceptoPago { get; set; }
        public virtual PeriodoAcademico? PeriodoAcademico { get; set; }
    }
}
