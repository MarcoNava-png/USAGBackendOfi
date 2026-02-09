namespace WebApplication2.Core.DTOs
{
    public class PlanEstudioDto
    {
        public int IdPlanEstudios { get; set; }

        public string ClavePlanEstudios { get; set; } = null!;

        public string? NombrePlanEstudios { get; set; }

        public string? RVOE { get; set; }

        public bool? PermiteAdelantar { get; set; }

        public string? Version { get; set; }

        public int? DuracionMeses { get; set; }

        public int MinimaAprobatoriaParcial { get; set; }

        public int MinimaAprobatoriaFinal { get; set; }

        public string Periodicidad { get; set; }

        public int IdPeriodicidad { get; set; }

        public int IdNivelEducativo { get; set; }

        public int IdCampus { get; set; }

        public string? NombreCampus { get; set; }

        public bool Activo { get; set; }
    }
}
