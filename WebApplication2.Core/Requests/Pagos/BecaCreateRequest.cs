using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.Pagos
{
    public class BecaCreateRequest
    {
        public int IdEstudiante { get; set; }
        public int? IdConceptoPago { get; set; }                   
        public string Tipo { get; set; }                           
        public decimal Valor { get; set; }                         
        public decimal? TopeMensual { get; set; }
        public DateOnly VigenciaDesde { get; set; }
        public DateOnly? VigenciaHasta { get; set; }
        public bool Activo { get; set; } = true;
        public string? Observaciones { get; set; }
    }
}
