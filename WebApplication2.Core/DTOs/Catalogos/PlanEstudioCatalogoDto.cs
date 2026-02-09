namespace WebApplication2.Core.DTOs.Catalogos
{
    public class PlanEstudioCatalogoDto
    {
        public int IdPlanEstudios { get; set; }
        public string ClavePlanEstudios { get; set; } = string.Empty;
        public string NombrePlanEstudios { get; set; } = string.Empty;
        public string NivelEducativo { get; set; } = string.Empty;
        public string Periodicidad { get; set; } = string.Empty;
        public string? RVOE { get; set; }
        public int? DuracionMeses { get; set; }
    }
}
