namespace WebApplication2.Core.Models.Titulacion;

public class CatalogoGeneroSEP
{
    public int Id { get; set; }
    public string IdGenero { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public bool Activo { get; set; } = true;
}
