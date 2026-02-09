namespace WebApplication2.Core.Models;

public partial class Convenio : BaseEntity
{
    public int IdConvenio { get; set; }

    public string ClaveConvenio { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string TipoBeneficio { get; set; } = null!;

    public decimal? DescuentoPct { get; set; }

    public decimal? Monto { get; set; }

    public DateOnly? VigenteDesde { get; set; }

    public DateOnly? VigenteHasta { get; set; }

    public string AplicaA { get; set; } = "TODOS";

    public int? MaxAplicaciones { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<AspiranteConvenio> AspiranteConvenio { get; set; } = new List<AspiranteConvenio>();

    public virtual ICollection<ConvenioAlcance> ConvenioAlcance { get; set; } = new List<ConvenioAlcance>();
}
