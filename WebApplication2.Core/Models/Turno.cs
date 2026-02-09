namespace WebApplication2.Core.Models;

public partial class Turno
{
    public int IdTurno { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Grupo> Grupo { get; set; } = new List<Grupo>();
}
