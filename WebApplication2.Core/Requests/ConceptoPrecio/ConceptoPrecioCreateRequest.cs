using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.ConceptoPrecio
{
    public class ConceptoPrecioCreateRequest
    {
        public int IdCampus { get; set; }
        public int IdPlanEstudios { get; set; }
        public string Moneda { get; set; } = "MXN";
        public decimal Importe { get; set; }
        public DateOnly VigenciaDesde { get; set; }
        public DateOnly? VigenciaHasta { get; set; }
        public bool Activo { get; set; } = true;
    }
}
