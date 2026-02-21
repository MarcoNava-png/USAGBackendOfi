namespace WebApplication2.Core.Models;

public partial class PlanModalidadDia
{
    public int IdPlanModalidadDia { get; set; }

    public int IdPlanEstudios { get; set; }

    public int IdModalidad { get; set; }

    public byte IdDiaSemana { get; set; }

    public virtual PlanEstudios IdPlanEstudiosNavigation { get; set; } = null!;

    public virtual Modalidad IdModalidadNavigation { get; set; } = null!;

    public virtual DiaSemana IdDiaSemanaNavigation { get; set; } = null!;
}
