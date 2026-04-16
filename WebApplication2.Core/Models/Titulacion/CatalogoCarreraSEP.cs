namespace WebApplication2.Core.Models.Titulacion;

public class CatalogoCarreraSEP
{
    public int Id { get; set; }
    public string IdNombreInstitucion { get; set; } = null!;
    public string IdNivelEstudios { get; set; } = null!;
    public string IdCarrera { get; set; } = null!;
    public string? ClaveCarrera { get; set; }
    public string NombreCarrera { get; set; } = null!;
    public bool Activo { get; set; } = true;
}
