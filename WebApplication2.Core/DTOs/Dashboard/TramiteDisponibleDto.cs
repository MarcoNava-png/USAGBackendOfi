namespace WebApplication2.Core.DTOs.Dashboard
{
    public class TramiteDisponibleDto
    {
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Link { get; set; } = string.Empty;
    }
}
