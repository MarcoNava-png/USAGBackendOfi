namespace WebApplication2.Core.DTOs.Grupo;

public class SincronizacionInscripcionesResultDto
{
    public int IdGrupo { get; set; }
    public string? NombreGrupo { get; set; }
    public string? CodigoGrupo { get; set; }
    public int TotalEstudiantes { get; set; }
    public int TotalMaterias { get; set; }
    public int InscripcionesCreadas { get; set; }
    public bool YaEstabaSincronizado { get; set; }
}
