namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class IngresosPorTenantDto
    {
        public int IdTenant { get; set; }
        public string Codigo { get; set; } = null!;
        public string NombreCorto { get; set; } = null!;
        public string ColorPrimario { get; set; } = "#14356F";
        public decimal IngresosMes { get; set; }
        public decimal IngresosAnio { get; set; }
        public decimal Adeudo { get; set; }
        public int RecibosEmitidos { get; set; }
        public int RecibosPagados { get; set; }
    }
}
