using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Models
{
    public class PlaneacionDocente : BaseEntity
    {
        public int Id { get; set; }

        public int IdProfesor { get; set; }
        public Profesor? Profesor { get; set; }

        public int IdGrupoMateria { get; set; }
        public GrupoMateria? GrupoMateria { get; set; }

        [MaxLength(255)]
        public string NombreArchivo { get; set; } = null!;

        [MaxLength(500)]
        public string UrlArchivo { get; set; } = null!;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [MaxLength(50)]
        public string? TipoArchivo { get; set; }

        public long TamanoBytes { get; set; }

        public DateTime FechaSubida { get; set; }
    }
}
