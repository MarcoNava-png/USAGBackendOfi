namespace WebApplication2.Core.Models.Titulacion;

public class CatalogoCargoSEP
{
    public int Id { get; set; }
    public string IdCargo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public bool Activo { get; set; } = true;
}
