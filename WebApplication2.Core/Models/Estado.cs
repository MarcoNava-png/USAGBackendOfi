namespace WebApplication2.Core.Models
{
    public class Estado
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Abreviatura { get; set; }
        public IEnumerable<Municipio> Municipios { get; set; }
    }
}
