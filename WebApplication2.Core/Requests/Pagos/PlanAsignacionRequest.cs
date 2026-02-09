using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.Pagos
{
    public class PlanAsignacionRequest
    {
        public int IdPlanPago { get; set; }
        public int IdEstudiante { get; set; }
        public string? Observaciones { get; set; }
    }
}
