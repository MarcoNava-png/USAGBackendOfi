namespace WebApplication2.Core.Responses.Caja
{
    public class QuitarRecargoResultado
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = "";
        public decimal RecargoCondonado { get; set; }
    }
}
