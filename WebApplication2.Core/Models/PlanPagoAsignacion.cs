using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Models
{
    public class PlanPagoAsignacion : BaseEntity
    {
        public long IdPlanPagoAsignacion { get; set; }
        public int IdPlanPago { get; set; }
        public int IdEstudiante { get; set; }
        public DateTime FechaAsignacionUtc { get; set; } = DateTime.UtcNow;
        public string? Observaciones { get; set; }
        public PlanPago PlanPago { get; set; } = null!;
    }

}
