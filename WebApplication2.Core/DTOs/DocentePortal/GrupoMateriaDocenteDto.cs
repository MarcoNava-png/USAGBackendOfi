namespace WebApplication2.Core.DTOs.DocentePortal;

public class GrupoMateriaDocenteDto
{
    public int IdGrupoMateria { get; set; }
    public string NombreMateria { get; set; } = null!;
    public string? ClaveMateria { get; set; }
    public string? CodigoGrupo { get; set; }
    public string? NombreGrupo { get; set; }
    public string? PlanEstudios { get; set; }
    public int? NumeroCuatrimestre { get; set; }
    public string? Aula { get; set; }
    public int TotalInscritos { get; set; }
    public short Cupo { get; set; }
    public string? PeriodoNombre { get; set; }
    public List<HorarioItemDto> Horarios { get; set; } = [];
}
