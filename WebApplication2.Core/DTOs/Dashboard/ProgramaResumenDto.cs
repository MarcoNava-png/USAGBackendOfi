namespace WebApplication2.Core.DTOs.Dashboard
{
    public class ProgramaResumenDto
    {
        public int IdPlanEstudios { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int TotalEstudiantes { get; set; }
        public decimal TasaRetencion { get; set; }
        public decimal PromedioGeneral { get; set; }
    }
}
