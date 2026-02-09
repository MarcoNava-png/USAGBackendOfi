namespace WebApplication2.Core.DTOs.Dashboard
{
    public class FechaImportanteDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public int DiasRestantes { get; set; }
        public string Tipo { get; set; } = string.Empty;
    }
}
