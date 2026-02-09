namespace WebApplication2.Core.Models;

public partial class DiaSemana
{
    public byte IdDiaSemana { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Horario> Horario { get; set; } = new List<Horario>();
}
