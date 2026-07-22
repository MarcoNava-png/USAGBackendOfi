namespace WebApplication2.Core.Models;

public partial class EstudioServicioVivienda
{
    public int IdEstudioSocioeconomico { get; set; }

    public int IdServicioVivienda { get; set; }

    public virtual EstudioSocioeconomico IdEstudioSocioeconomicoNavigation { get; set; } = null!;

    public virtual CatServicioVivienda IdServicioViviendaNavigation { get; set; } = null!;
}
