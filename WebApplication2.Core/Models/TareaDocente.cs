using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Models
{
    public class TareaDocente : BaseEntity
    {
        public int Id { get; set; }

        public int IdGrupoMateria { get; set; }
        public GrupoMateria? GrupoMateria { get; set; }

        public int IdProfesor { get; set; }
        public Profesor? Profesor { get; set; }

        [MaxLength(200)]
        public string Titulo { get; set; } = null!;

        [MaxLength(2000)]
        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; }
        public DateTime FechaLimite { get; set; }

        public decimal PuntosMaximos { get; set; }
        public bool Activa { get; set; } = true;

        public ICollection<EntregaTarea> Entregas { get; set; } = new List<EntregaTarea>();
    }
}
