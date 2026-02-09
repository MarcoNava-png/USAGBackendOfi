namespace WebApplication2.Core.Responses.Caja
{
    public class ModificarRecargoResultado
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = "";
        public decimal RecargoAnterior { get; set; }
        public decimal RecargoNuevo { get; set; }
        public decimal NuevoTotal { get; set; }
    }
}
