namespace WebApplication2.Core.Models;

public class PlantillaReporte : BaseEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Codigo { get; set; } = null!;
    public string? Descripcion { get; set; }
    public string Categoria { get; set; } = null!;
    public string RutaArchivo { get; set; } = null!;
    public string NombreArchivoOriginal { get; set; } = null!;
    public string VariablesDisponibles { get; set; } = "[]";
    public string? Origen { get; set; }
    public string? RolesGenera { get; set; }
    public bool Activa { get; set; } = true;
}
