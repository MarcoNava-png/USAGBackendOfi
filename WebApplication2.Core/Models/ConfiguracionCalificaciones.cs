namespace WebApplication2.Core.Models
{
    public class ConfiguracionCalificaciones : BaseEntity
    {
        public int IdConfiguracionCalificaciones { get; set; }
        public decimal EscalaMaxima { get; set; } = 10m;
        public decimal CalificacionMinimaAprobatoria { get; set; } = 6m;
        public int Decimales { get; set; } = 1;
        public bool RedondearAlEntero { get; set; } = false;
    }
}
