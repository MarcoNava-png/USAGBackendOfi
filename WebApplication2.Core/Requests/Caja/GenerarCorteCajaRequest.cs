namespace WebApplication2.Core.Requests.Caja
{
    public class GenerarCorteCajaRequest
    {
        public string? IdUsuarioCaja { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
