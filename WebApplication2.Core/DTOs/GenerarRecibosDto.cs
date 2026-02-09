using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class GenerarRecibosDto
    {
        public int IdEstudiante { get; set; }
        public int IdPeriodoAcademico { get; set; }
        public int? IdPlanPago { get; set; }
        public EstrategiaEmisionEnum estrategia { get; set; } 
        public byte DiaVencimiento { get; set; }
        public bool AplicarBecas { get; set; } = true;
    }
}
