namespace WebApplication2.Core.Models;

public partial class NivelEducativo
{
    public int IdNivelEducativo { get; set; }

    public string DescNivelEducativo { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<PlanEstudios> PlanEstudios { get; set; } = new List<PlanEstudios>();
}
