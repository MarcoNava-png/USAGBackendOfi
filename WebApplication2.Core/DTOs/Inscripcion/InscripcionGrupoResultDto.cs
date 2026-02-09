namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class InscripcionGrupoResultDto
    {
        public int IdGrupo { get; set; }
        public string CodigoGrupo { get; set; } = string.Empty;
        public string NombreGrupo { get; set; } = string.Empty;
        public int IdEstudiante { get; set; }
        public string MatriculaEstudiante { get; set; } = string.Empty;
        public string NombreEstudiante { get; set; } = string.Empty;
        public int TotalMaterias { get; set; }
        public int MateriasInscritas { get; set; }
        public int MateriasFallidas { get; set; }
        public List<InscripcionMateriaDto> DetalleInscripciones { get; set; } = new();
        public ValidacionInscripcionGrupoDto Validaciones { get; set; } = new();
        public DateTime FechaInscripcion { get; set; }
        public bool InscripcionForzada { get; set; }
        public string? Observaciones { get; set; }
    }
}
