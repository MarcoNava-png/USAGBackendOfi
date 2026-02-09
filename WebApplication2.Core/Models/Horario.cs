namespace WebApplication2.Core.Models;

public partial class Horario : BaseEntity
{
    public int IdHorario { get; set; }

    public int IdGrupoMateria { get; set; }

    public byte IdDiaSemana { get; set; }

    public TimeOnly HoraInicio { get; set; }

    public TimeOnly HoraFin { get; set; }

    public string? Aula { get; set; }

    public virtual DiaSemana IdDiaSemanaNavigation { get; set; } = null!;

    public virtual GrupoMateria IdGrupoMateriaNavigation { get; set; } = null!;
}
