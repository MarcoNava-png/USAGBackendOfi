namespace WebApplication2.Core.DTOs.DocentePortal
{
    public class EntregaTareaDto
    {
        public int Id { get; set; }
        public int IdTarea { get; set; }
        public int IdEstudiante { get; set; }
        public string NombreAlumno { get; set; } = null!;
        public string Matricula { get; set; } = null!;
        public string NombreArchivo { get; set; } = null!;
        public string UrlArchivo { get; set; } = null!;
        public string? TipoArchivo { get; set; }
        public long TamanoBytes { get; set; }
        public DateTime FechaEntrega { get; set; }
        public decimal? Calificacion { get; set; }
        public string? Retroalimentacion { get; set; }
        public bool Revisada { get; set; }
    }
}
