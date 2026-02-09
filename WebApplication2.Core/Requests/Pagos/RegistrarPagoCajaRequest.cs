using WebApplication2.Core.DTOs.Pagos;

namespace WebApplication2.Core.Requests.Pagos
{
    public class RegistrarPagoCajaRequest
    {
        public string? IdUsuarioCaja { get; set; }
        public int? IdCaja { get; set; }
        public DateTime FechaPago { get; set; } = DateTime.UtcNow;
        public int IdMedioPago { get; set; }
        public decimal Monto { get; set; }
        public string? Referencia { get; set; }
        public string? Notas { get; set; }
        public List<ReciboParaPago> RecibosSeleccionados { get; set; } = new();
        public DescuentoAutorizado? DescuentoAutorizado { get; set; }
    }
}
