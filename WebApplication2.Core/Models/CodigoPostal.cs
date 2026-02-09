namespace WebApplication2.Core.Models
{
    public class CodigoPostal
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Asentamiento { get; set; }
        public string MunicipioId { get; set; }
        public Municipio Municipio { get; set; }
    }
}
