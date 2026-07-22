namespace WebApplication2.Core.Models;

public partial class EstudioRecursoTecnologico
{
    public int IdEstudioSocioeconomico { get; set; }

    public int IdRecursoTecnologico { get; set; }

    public virtual EstudioSocioeconomico IdEstudioSocioeconomicoNavigation { get; set; } = null!;

    public virtual CatRecursoTecnologico IdRecursoTecnologicoNavigation { get; set; } = null!;
}
