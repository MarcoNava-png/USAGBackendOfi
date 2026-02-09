namespace WebApplication2.Core.Models;

public partial class EstadoCivil
{
    public int IdEstadoCivil { get; set; }

    public string DescEstadoCivil { get; set; } = null!;

    public virtual ICollection<Persona> Persona { get; set; } = new List<Persona>();
}
