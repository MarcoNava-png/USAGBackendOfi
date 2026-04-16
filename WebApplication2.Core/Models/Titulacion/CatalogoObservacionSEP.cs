namespace WebApplication2.Core.Models.Titulacion;

public class CatalogoObservacionSEP
{
    public int Id { get; set; }
    public string IdObservacion { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public bool Activo { get; set; } = true;
}
