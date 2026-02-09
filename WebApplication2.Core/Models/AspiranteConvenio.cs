namespace WebApplication2.Core.Models;

public partial class AspiranteConvenio : BaseEntity
{
    public int IdAspiranteConvenio { get; set; }

    public int IdAspirante { get; set; }

    public int IdConvenio { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public string Estatus { get; set; } = null!;

    public string? Evidencia { get; set; }

    public int VecesAplicado { get; set; } = 0;

    public virtual Aspirante IdAspiranteNavigation { get; set; } = null!;

    public virtual Convenio IdConvenioNavigation { get; set; } = null!;
}
