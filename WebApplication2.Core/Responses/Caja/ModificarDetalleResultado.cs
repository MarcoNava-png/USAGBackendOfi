namespace WebApplication2.Core.Responses.Caja
{
    public class ModificarDetalleResultado
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = "";
        public decimal MontoAnterior { get; set; }
        public decimal MontoNuevo { get; set; }
        public decimal NuevoTotal { get; set; }
        public decimal NuevoSaldo { get; set; }
    }
}
