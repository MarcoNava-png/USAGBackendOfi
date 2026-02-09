namespace WebApplication2.Core.DTOs
{
    public class CampusDto
    {
        public int IdCampus { get; set; }

        public string ClaveCampus { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public string Direccion { get; set; }
        public bool Activo { get; set; }
    }
}
