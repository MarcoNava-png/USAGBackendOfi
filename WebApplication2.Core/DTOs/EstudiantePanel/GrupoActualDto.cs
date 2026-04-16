namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class GrupoActualDto
    {
        public int IdGrupo { get; set; }
        public int? IdEstudianteGrupo { get; set; }
        public string CodigoGrupo { get; set; } = string.Empty;
        public string? NombreGrupo { get; set; }
        public string? Turno { get; set; }
        public int? CupoMaximo { get; set; }
        public int? AlumnosInscritos { get; set; }
        public int? NumeroCuatrimestre { get; set; }
        public int? IdPlanEstudios { get; set; }
        public int? IdPeriodoAcademico { get; set; }
    }
}
