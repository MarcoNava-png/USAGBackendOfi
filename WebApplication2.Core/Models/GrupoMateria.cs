namespace WebApplication2.Core.Models;

public partial class GrupoMateria : BaseEntity
{
    public int IdGrupoMateria { get; set; }
    public string Name { get; set; }
    public int IdGrupo { get; set; }

    public int IdMateriaPlan { get; set; }

    public int? IdProfesor { get; set; }

    public string? Aula { get; set; }

    public short Cupo { get; set; }

    public virtual ICollection<Horario> Horario { get; set; } = new List<Horario>();

    public virtual Grupo IdGrupoNavigation { get; set; } = null!;

    public virtual MateriaPlan IdMateriaPlanNavigation { get; set; } = null!;

    public virtual Profesor? IdProfesorNavigation { get; set; }

    public virtual ICollection<Inscripcion> Inscripcion { get; set; } = new List<Inscripcion>();
}
