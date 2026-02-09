namespace WebApplication2.Core.Models;

public partial class MedioContacto : BaseEntity
{
    public int IdMedioContacto { get; set; }

    public string DescMedio { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Aspirante> Aspirante { get; set; } = new List<Aspirante>();
}
