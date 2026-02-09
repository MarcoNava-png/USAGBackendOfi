namespace WebApplication2.Core.Models;

public partial class PeriodoAcademico : BaseEntity
{
    public int IdPeriodoAcademico { get; set; }

    public string Clave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public int IdPeriodicidad { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public bool EsPeriodoActual { get; set; }

    public virtual ICollection<Grupo> Grupo { get; set; } = new List<Grupo>();

    public virtual Periodicidad IdPeriodicidadNavigation { get; set; } = null!;
}
