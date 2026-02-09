namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class IngresosMensualesDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = null!;
        public decimal Total { get; set; }
        public int CantidadRecibos { get; set; }
    }
}
