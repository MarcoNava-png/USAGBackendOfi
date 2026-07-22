namespace WebApplication2.Core.Models
{
    public class ReporteDefinicion : BaseEntity
    {
        public int IdReporteDefinicion { get; set; }
        public string Nombre { get; set; } = null!;
        public string Fuente { get; set; } = null!;
        public string ColumnasJson { get; set; } = "[]";
        public string FiltrosJson { get; set; } = "[]";
        public string? OrdenCampo { get; set; }
        public bool OrdenDescendente { get; set; }
        public string? AgruparPor { get; set; }
    }
}
