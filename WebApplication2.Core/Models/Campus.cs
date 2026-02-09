namespace WebApplication2.Core.Models;

public partial class Campus : BaseEntity
{
    public int IdCampus { get; set; }

    public string ClaveCampus { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int? IdDireccion { get; set; }

    public string? Telefono { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<ConvenioAlcance> ConvenioAlcance { get; set; } = new List<ConvenioAlcance>();

    public virtual Direccion? IdDireccionNavigation { get; set; }

    public virtual ICollection<PlanEstudios> PlanEstudios { get; set; } = new List<PlanEstudios>();
}
