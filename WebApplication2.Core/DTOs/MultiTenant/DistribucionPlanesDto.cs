namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class DistribucionPlanesDto
    {
        public int IdPlan { get; set; }
        public string NombrePlan { get; set; } = null!;
        public int CantidadTenants { get; set; }
        public decimal IngresoMensual { get; set; }
        public decimal Porcentaje { get; set; }
    }
}
