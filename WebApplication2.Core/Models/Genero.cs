namespace WebApplication2.Core.Models;

public partial class Genero
{
    public int IdGenero { get; set; }

    public string DescGenero { get; set; } = null!;

    public virtual ICollection<Persona> Persona { get; set; } = new List<Persona>();
}
