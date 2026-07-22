namespace WebApplication2.Core.Models;

public partial class CatServicioMedico : BaseEntity
{
    public int IdServicioMedico { get; set; }

    public string Nombre { get; set; } = null!;

    public int Orden { get; set; }

    public virtual ICollection<EstudioSocioeconomico> EstudiosSocioeconomicos { get; set; } = new List<EstudioSocioeconomico>();
}
