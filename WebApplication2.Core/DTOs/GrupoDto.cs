namespace WebApplication2.Core.DTOs
{
    public class GrupoDto
    {
        public int IdGrupo { get; set; }

        public string NombreGrupo { get; set; } = null!;

        public int IdPlanEstudios { get; set; }

        public string PlanEstudios { get; set; }

        public int? IdCampus { get; set; }

        public string? Campus { get; set; }

        public string PeriodoAcademico { get; set; }

        public DateOnly? PeriodoInicio { get; set; }

        public DateOnly? PeriodoFin { get; set; }

        public byte ConsecutivoPeriodicidad { get; set; }

        public byte NumeroGrupo { get; set; }

        public string Turno { get; set; }

        public short CapacidadMaxima { get; set; }

        public string CodigoGrupo { get; set; }

        public int EstudiantesInscritos { get; set; }
    }
}
