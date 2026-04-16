namespace WebApplication2.Core.Models.Titulacion;

public class CatalogoAsignaturaSEP
{
    public int Id { get; set; }
    public string IdNombreInstitucion { get; set; } = null!;
    public int IdCarrera { get; set; }
    public int IdAsignatura { get; set; }
    public string? ClaveAsignatura { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal? Creditos { get; set; }
    public int? IdTipoAsignatura { get; set; }
    public string? TipoAsignatura { get; set; }
    public bool Activo { get; set; } = true;
}
