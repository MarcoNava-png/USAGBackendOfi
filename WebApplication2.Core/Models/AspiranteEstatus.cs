namespace WebApplication2.Core.Models;

public partial class AspiranteEstatus : BaseEntity
{
    public int IdAspiranteEstatus { get; set; }

    public string DescEstatus { get; set; } = null!;

    public virtual ICollection<Aspirante> Aspirante { get; set; } = new List<Aspirante>();
}
