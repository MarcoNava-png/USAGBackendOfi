namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class InscripcionAspiranteResultDto
    {
        public int IdAspirante { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string NuevoEstatusAspirante { get; set; } = string.Empty;
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateOnly FechaIngreso { get; set; }
        public string PlanEstudios { get; set; } = string.Empty;
        public CredencialesAccesoDto Credenciales { get; set; } = new();
        public List<ReciboGeneradoDto> RecibosGenerados { get; set; } = new();
        public ValidacionesInscripcionDto Validaciones { get; set; } = new();
        public DateTime FechaProceso { get; set; }
        public string? UsuarioQueProceso { get; set; }
        public bool InscripcionForzada { get; set; }
    }
}
