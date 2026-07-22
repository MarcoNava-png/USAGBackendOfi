namespace WebApplication2.Core.Models;

public partial class CatServicioVivienda : BaseEntity
{
    public int IdServicioVivienda { get; set; }

    public string Nombre { get; set; } = null!;

    public int Orden { get; set; }

    public virtual ICollection<EstudioServicioVivienda> EstudioServicioVivienda { get; set; } = new List<EstudioServicioVivienda>();
}
