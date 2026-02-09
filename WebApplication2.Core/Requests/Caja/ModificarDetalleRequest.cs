namespace WebApplication2.Core.Requests.Caja
{
    public class ModificarDetalleRequest
    {
        public decimal NuevoMonto { get; set; }
        public string Motivo { get; set; } = "Ajuste de monto autorizado";
    }
}
