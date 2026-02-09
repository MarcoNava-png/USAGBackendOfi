using WebApplication2.Core.DTOs.Grupo;

namespace WebApplication2.Core.Responses.Grupo
{
    public class EstudiantesDelGrupoResponse
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public string? CodigoGrupo { get; set; }
        public string PlanEstudios { get; set; } = string.Empty;
        public string PeriodoAcademico { get; set; } = string.Empty;
        public int NumeroCuatrimestre { get; set; }
        public int TotalEstudiantes { get; set; }
        public int CapacidadMaxima { get; set; }
        public int CupoDisponible { get; set; }
        public List<EstudianteEnGrupoDto> Estudiantes { get; set; } = new();
    }
}
