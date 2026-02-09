using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs
{
    public class PagoDto
    {
        public long IdPago { get; set; }
        public string? FolioPago { get; set; }
        public DateTime FechaPagoUtc { get; set; }
        public int IdMedioPago { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; }
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
        public EstatusPago Estatus { get; set; }
        public string? MedioPago { get; set; }

        public int? IdEstudiante { get; set; }
        public string? Matricula { get; set; }
        public string? NombreEstudiante { get; set; }
        public string? Concepto { get; set; }
        public string? FolioRecibo { get; set; }
    }

}
