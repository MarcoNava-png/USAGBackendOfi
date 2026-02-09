using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.Requests.Pagos
{
    public class PlanDetalleUpdateRequest : PlanDetalleCreateRequest
    {
        public int IdPlanPagoDetalle { get; set; }
    }
}
