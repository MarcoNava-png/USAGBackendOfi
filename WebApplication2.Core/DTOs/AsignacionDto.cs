using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class AsignacionDto
    {
        public long IdPlanPagoAsignacion { get; set; }
        public int IdPlanPago { get; set; }
        public int IdEstudiante { get; set; }
        public DateTime FechaAsignacionUtc { get; set; }
        public string? Observaciones { get; set; }
    }
}
