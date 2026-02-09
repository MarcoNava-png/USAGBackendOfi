namespace WebApplication2.Core.Models
{
    public class Municipio
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string EstadoId { get; set; }
        public Estado Estado { get; set; }
        public IEnumerable<CodigoPostal> CodigosPostales { get; set; }
    }
}
