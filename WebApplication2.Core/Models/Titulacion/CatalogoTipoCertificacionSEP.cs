namespace WebApplication2.Core.Models.Titulacion;

public class CatalogoTipoCertificacionSEP
{
    public int Id { get; set; }
    public string IdTipoCertificacion { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public bool Activo { get; set; } = true;
}
