namespace WebApplication2.Core.Models;

public partial class Grupo : BaseEntity
{
    public int IdGrupo { get; set; }
    public string NombreGrupo { get; set; }

    public int IdPlanEstudios { get; set; }

    public int IdPeriodoAcademico { get; set; }

    public byte NumeroCuatrimestre { get; set; }

    public byte NumeroGrupo { get; set; }

    public int IdTurno { get; set; }

    public short CapacidadMaxima { get; set; }

    public string? CodigoGrupo { get; set; }

    public virtual ICollection<GrupoMateria> GrupoMateria { get; set; } = new List<GrupoMateria>();

    public virtual ICollection<EstudianteGrupo> EstudianteGrupo { get; set; } = new List<EstudianteGrupo>();

    public virtual PeriodoAcademico IdPeriodoAcademicoNavigation { get; set; } = null!;

    public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;

    public virtual Turno IdTurnoNavigation { get; set; } = null!;
}
