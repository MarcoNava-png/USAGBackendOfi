namespace WebApplication2.Core.Models;

public partial class Materia : BaseEntity
{
    public int IdMateria { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public decimal Creditos { get; set; }

    public byte HorasTeoria { get; set; }

    public byte HorasPractica { get; set; }

    public bool Activa { get; set; }

    public virtual ICollection<MateriaPlan> MateriaPlan { get; set; } = new List<MateriaPlan>();
}
