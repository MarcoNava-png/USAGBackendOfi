using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public class PlanPago : BaseEntity
    {
        public int IdPlanPago { get; set; }
        public string Nombre { get; set; } = null!;
        public int IdPeriodicidad { get; set; }
        public int IdPeriodoAcademico { get; set; }
        public int? IdPlanEstudios { get; set; }
        public ModalidadPlanEnum Modalidad { get; set; }
        public string Moneda { get; set; } = "MXN";
        public bool Activo { get; set; } = true;
        public DateOnly VigenciaDesde { get; set; }
        public DateOnly? VigenciaHasta { get; set; }

        public ICollection<PlanPagoDetalle> Detalles { get; set; } = new List<PlanPagoDetalle>();
        public ICollection<PlanPagoAsignacion> Asignaciones { get; set; } = new List<PlanPagoAsignacion>();
    }
}
