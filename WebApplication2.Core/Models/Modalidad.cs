namespace WebApplication2.Core.Models;

public partial class Modalidad
{
    public int IdModalidad { get; set; }

    public string DescModalidad { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Aspirante> Aspirante { get; set; } = new List<Aspirante>();

    public virtual ICollection<PlantillaCobro> PlantillaCobro { get; set; } = new List<PlantillaCobro>();
}
