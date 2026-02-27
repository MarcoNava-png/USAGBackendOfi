using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Models
{
    public class EntregaTarea : BaseEntity
    {
        public int Id { get; set; }

        public int IdTarea { get; set; }
        public TareaDocente? Tarea { get; set; }

        public int IdEstudiante { get; set; }
        public Estudiante? Estudiante { get; set; }

        [MaxLength(255)]
        public string NombreArchivo { get; set; } = null!;

        [MaxLength(500)]
        public string UrlArchivo { get; set; } = null!;

        [MaxLength(50)]
        public string? TipoArchivo { get; set; }

        public long TamanoBytes { get; set; }

        public DateTime FechaEntrega { get; set; }

        public decimal? Calificacion { get; set; }

        [MaxLength(1000)]
        public string? Retroalimentacion { get; set; }

        public DateTime? FechaRevision { get; set; }
        public bool Revisada { get; set; }
    }
}
