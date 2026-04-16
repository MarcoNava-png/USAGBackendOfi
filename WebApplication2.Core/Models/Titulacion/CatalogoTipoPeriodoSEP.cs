namespace WebApplication2.Core.Models.Titulacion;

public class CatalogoTipoPeriodoSEP
{
    public int Id { get; set; }
    public string IdTipoPeriodo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public bool Activo { get; set; } = true;
}
