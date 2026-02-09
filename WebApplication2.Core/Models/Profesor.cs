using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Core.Models;

public partial class Profesor : BaseEntity
{
    public int IdProfesor { get; set; }

    public string NoEmpleado { get; set; } = null!;

    public int IdPersona { get; set; }

    public string? EmailInstitucional { get; set; }

    public bool Activo { get; set; }

    public string? UsuarioId { get; set; }

    public int? CampusId { get; set; }

    public virtual ICollection<GrupoMateria> GrupoMateria { get; set; } = new List<GrupoMateria>();

    public virtual Persona IdPersonaNavigation { get; set; } = null!;
    
    public virtual ApplicationUser Usuario { get; set; }

    public virtual Campus? Campus { get; set; }
}
