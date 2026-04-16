namespace WebApplication2.Core.Models.Titulacion;

public class CatalogoNivelEstudiosSEP
{
    public int Id { get; set; }
    public string IdNivelEstudios { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public bool Activo { get; set; } = true;
}
