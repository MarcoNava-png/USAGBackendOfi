namespace WebApplication2.Core.Models;

public partial class ModalidadPlan
{
    public int IdModalidadPlan { get; set; }

    public string DescModalidadPlan { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<PlanPago> PlanPago { get; set; } = new List<PlanPago>();
}
