namespace WebApplication2.Core.DTOs.Documentos
{
    public class ConstanciaEstudiosDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string PlanEstudios { get; set; } = string.Empty;
        public string? RVOE { get; set; }
        public string PeriodoActual { get; set; } = string.Empty;
        public string Grado { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
        public string Campus { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public bool IncluyeMaterias { get; set; }
        public List<ConstanciaMateriaDto> Materias { get; set; } = new();
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string FolioDocumento { get; set; } = string.Empty;
        public Guid CodigoVerificacion { get; set; }
        public string UrlVerificacion { get; set; } = string.Empty;
    }
}
