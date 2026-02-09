namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class GrupoResumenDto
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public string CodigoGrupo { get; set; } = string.Empty;
        public int NumeroGrupo { get; set; }
        public string Turno { get; set; } = string.Empty;
        public int IdTurno { get; set; }
        public string PeriodoAcademico { get; set; } = string.Empty;
        public int IdPeriodoAcademico { get; set; }
        public int CapacidadMaxima { get; set; }
        public int TotalEstudiantes { get; set; }
        public int CupoDisponible { get; set; }
        public bool TieneCupo { get; set; }
        public int TotalMaterias { get; set; }
    }
}
