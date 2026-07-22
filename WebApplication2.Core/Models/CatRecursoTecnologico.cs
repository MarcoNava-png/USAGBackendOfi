namespace WebApplication2.Core.Models;

public partial class CatRecursoTecnologico : BaseEntity
{
    public int IdRecursoTecnologico { get; set; }

    public string Nombre { get; set; } = null!;

    public int Orden { get; set; }

    public virtual ICollection<EstudioRecursoTecnologico> EstudioRecursoTecnologico { get; set; } = new List<EstudioRecursoTecnologico>();
}
