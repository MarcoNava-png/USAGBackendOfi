namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ReporteIngresosGlobalDto
    {
        public decimal IngresosTotalMes { get; set; }
        public decimal IngresosTotalAnio { get; set; }
        public decimal AdeudoTotalGlobal { get; set; }
        public List<IngresosPorTenantDto> IngresosPorTenant { get; set; } = new();
        public List<IngresosMensualesDto> TendenciaAnual { get; set; } = new();
    }
}
