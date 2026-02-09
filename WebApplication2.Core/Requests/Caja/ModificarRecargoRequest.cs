namespace WebApplication2.Core.Requests.Caja
{
    public class ModificarRecargoRequest
    {
        public decimal NuevoRecargo { get; set; }
        public string Motivo { get; set; } = "Ajuste de recargo autorizado";
    }
}
