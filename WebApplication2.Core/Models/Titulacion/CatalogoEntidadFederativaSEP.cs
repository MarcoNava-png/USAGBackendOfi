namespace WebApplication2.Core.Models.Titulacion;

public class CatalogoEntidadFederativaSEP
{
    public int Id { get; set; }
    public string IdEntidadFederativa { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; } = true;
}
