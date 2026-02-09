namespace WebApplication2.Core.Models;

public partial class ConvenioAlcance : BaseEntity
{
    public int IdConvenioAlcance { get; set; }

    public int IdConvenio { get; set; }

    public int? IdCampus { get; set; }

    public int? IdPlanEstudios { get; set; }

    public DateOnly? VigenteDesde { get; set; }

    public DateOnly? VigenteHasta { get; set; }

    public virtual Campus? IdCampusNavigation { get; set; }

    public virtual Convenio IdConvenioNavigation { get; set; } = null!;

    public virtual PlanEstudios? IdPlanEstudiosNavigation { get; set; }
}
