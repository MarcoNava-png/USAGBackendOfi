namespace WebApplication2.Core.DTOs.Documentos
{
    public class KardexEstudianteDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string PlanEstudios { get; set; } = string.Empty;
        public string? RVOE { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public decimal PromedioGeneral { get; set; }
        public int CreditosCursados { get; set; }
        public int CreditosTotales { get; set; }
        public decimal PorcentajeAvance { get; set; }
        public List<KardexPeriodoDto> Periodos { get; set; } = new();
    }
}
