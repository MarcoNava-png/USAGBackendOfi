namespace WebApplication2.Core.Models;

public partial class MateriaPlan : BaseEntity
{
    public int IdMateriaPlan { get; set; }

    public int IdPlanEstudios { get; set; }

    public int IdMateria { get; set; }

    public byte Cuatrimestre { get; set; }

    public bool EsOptativa { get; set; }

    public virtual ICollection<GrupoMateria> GrupoMateria { get; set; } = new List<GrupoMateria>();

    public virtual Materia IdMateriaNavigation { get; set; } = null!;

    public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;
}
