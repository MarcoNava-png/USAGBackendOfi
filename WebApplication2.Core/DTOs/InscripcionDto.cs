namespace WebApplication2.Core.DTOs
{
    public class InscripcionDto
    {
        public int IdInscripcion { get; set; }

        public int IdEstudiante { get; set; }

        public int IdGrupoMateria { get; set; }

        public string? NombreGrupoMateria { get; set; }
        public string? NombreMateria { get; set; }
        public string? NombreGrupo { get; set; }
        public int? IdPeriodoAcademico { get; set; }

        public DateTime FechaInscripcion { get; set; }

        public string Estado { get; set; } = null!;

        public decimal? CalificacionFinal { get; set; }
    }
}
