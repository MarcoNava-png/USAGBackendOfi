namespace WebApplication2.Core.Models;

public class PlanDocumentoRequisito : BaseEntity
{
    public int IdPlanDocumentoRequisito { get; set; }

    public int IdPlanEstudios { get; set; }

    public int IdDocumentoRequisito { get; set; }

    public bool EsObligatorio { get; set; }

    public virtual PlanEstudios PlanEstudios { get; set; } = null!;

    public virtual DocumentoRequisito DocumentoRequisito { get; set; } = null!;
}
