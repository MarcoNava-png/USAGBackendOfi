using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication2.Core.DTOs
{
    public class CarteraResumenDto
    {
        public int IdPeriodoAcademico { get; set; }
        public int Recibos { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Descuento { get; set; }
        public decimal Recargos { get; set; }
        public decimal Total { get; set; }
        public decimal Cobrado { get; set; }
        public decimal Saldo { get; set; }
        public decimal TasaRecuperacionPct { get; set; }
    }
}
