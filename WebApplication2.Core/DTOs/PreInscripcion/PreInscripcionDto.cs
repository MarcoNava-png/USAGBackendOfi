namespace WebApplication2.Core.DTOs.PreInscripcion
{
    public class PreInscripcionDto
    {
        public int IdPreInscripcion { get; set; }
        public int IdEstudiante { get; set; }
        public string? Matricula { get; set; }
        public string? NombreCompleto { get; set; }
        public int IdPlanEstudios { get; set; }
        public string? ClavePlan { get; set; }
        public string? PlanEstudios { get; set; }
        public int IdPeriodoAcademicoDestino { get; set; }
        public string? PeriodoClave { get; set; }
        public string? PeriodoNombre { get; set; }
        public byte NumeroCuatrimestreObjetivo { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string? Nota { get; set; }
        public DateTime FechaApartado { get; set; }
    }
}
