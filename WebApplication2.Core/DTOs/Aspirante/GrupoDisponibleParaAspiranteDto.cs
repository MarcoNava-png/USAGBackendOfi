namespace WebApplication2.Core.DTOs.Aspirante;

public class GrupoDisponibleParaAspiranteDto
{
    public int IdGrupo { get; set; }
    public string NombreGrupo { get; set; } = string.Empty;
    public string? CodigoGrupo { get; set; }
    public byte NumeroCuatrimestre { get; set; }
    public string? Turno { get; set; }
    public int? IdTurno { get; set; }
    public int CapacidadMaxima { get; set; }
    public int Ocupados { get; set; }
    public int CupoDisponible { get; set; }
    public bool TieneCupo { get; set; }
    public string? PeriodoAcademico { get; set; }
    public int IdPeriodoAcademico { get; set; }
}
