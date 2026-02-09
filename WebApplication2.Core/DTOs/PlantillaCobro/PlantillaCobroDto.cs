namespace WebApplication2.Core.DTOs.PlantillaCobro
{
    public class PlantillaCobroDto
    {
        public int IdPlantillaCobro { get; set; }
        public string NombrePlantilla { get; set; } = null!;
        public int IdPlanEstudios { get; set; }
        public int NumeroCuatrimestre { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public int? IdTurno { get; set; }
        public int? IdModalidad { get; set; }
        public int Version { get; set; }
        public bool EsActiva { get; set; }
        public DateTime FechaVigenciaInicio { get; set; }
        public DateTime? FechaVigenciaFin { get; set; }
        public int EstrategiaEmision { get; set; }
        public int NumeroRecibos { get; set; }
        public int DiaVencimiento { get; set; }
        public string CreadoPor { get; set; } = null!;
        public DateTime FechaCreacion { get; set; }
        public string? ModificadoPor { get; set; }
        public DateTime? FechaModificacion { get; set; }

        public string? NombrePlanEstudios { get; set; }
        public string? NombrePeriodo { get; set; }
        public string? NombreTurno { get; set; }
        public string? NombreModalidad { get; set; }
        public decimal? TotalConceptos { get; set; }

        public List<PlantillaCobroDetalleDto>? Detalles { get; set; }
    }
}
