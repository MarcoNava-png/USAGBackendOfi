namespace WebApplication2.Core.DTOs.PortalAlumno
{
    public class MiAsistenciaDto
    {
        public decimal PorcentajeGeneral { get; set; }
        public int TotalClases { get; set; }
        public int Asistencias { get; set; }
        public int Faltas { get; set; }
        public int Retardos { get; set; }
        public int Justificadas { get; set; }
        public List<MiAsistenciaMateriaDto> Materias { get; set; } = new();
    }

    public class MiAsistenciaMateriaDto
    {
        public int IdMateria { get; set; }
        public string ClaveMateria { get; set; } = string.Empty;
        public string NombreMateria { get; set; } = string.Empty;
        public string? GrupoCodigo { get; set; }
        public decimal Porcentaje { get; set; }
        public int TotalClases { get; set; }
        public int Asistencias { get; set; }
        public int Faltas { get; set; }
        public int Retardos { get; set; }
        public int Justificadas { get; set; }
        public List<MiAsistenciaDetalleDto> Detalles { get; set; } = new();
    }

    public class MiAsistenciaDetalleDto
    {
        public DateOnly Fecha { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public string? Observacion { get; set; }
    }
}
