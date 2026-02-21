using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WebApplication2.Core.DTOs
{
    public class CrearPlanPagoDto
    {
        public string Nombre { get; set; }
        public int IdPeriodicidad { get; set; }
        public int IdPeriodoAcademico { get; set; }
        public int? IdPlanEstudios { get; set; }
        public int idModalidadPlan { get; set; }
        public string Moneda { get; set; }        
        public DateOnly VigenciaDesde { get; set; }
        public DateOnly? VigenciaHasta { get; set; }
        public bool Activo { get; set; } = true;
    }
}
