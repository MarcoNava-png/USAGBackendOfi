using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.Pagos
{
    public class RecargoPoliticaRequest
    {
        public int? IdCampus { get; set; }
        public int? IdPlanEstudios { get; set; }
        public decimal TasaDiaria { get; set; }                     
        public byte DiaInicioGracia { get; set; } = 1;
        public byte DiaFinGracia { get; set; } = 5;
        public decimal? RecargoMinimo { get; set; }
        public decimal? RecargoMaximo { get; set; }
        public int? TopeDiasMora { get; set; }
        public bool Activo { get; set; } = true;
    }
}
