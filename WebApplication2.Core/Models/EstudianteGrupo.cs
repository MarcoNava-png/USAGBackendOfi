namespace WebApplication2.Core.Models;


public partial class EstudianteGrupo : BaseEntity
{
    public int IdEstudianteGrupo { get; set; }

    public int IdEstudiante { get; set; }

    public int IdGrupo { get; set; }

    public DateTime FechaInscripcion { get; set; }

    public string Estado { get; set; } = "Inscrito";

    public string? Observaciones { get; set; }

    public virtual Estudiante IdEstudianteNavigation { get; set; } = null!;
    public virtual Grupo IdGrupoNavigation { get; set; } = null!;
}
