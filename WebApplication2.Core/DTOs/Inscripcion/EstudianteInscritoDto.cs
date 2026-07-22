namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class EstudianteInscritoDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? PlanEstudios { get; set; }
        public int? IdInscripcion { get; set; }
        public int MateriasInscritas { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public string? Estado { get; set; }
        public bool Activo { get; set; }
        public int EstatusAcademico { get; set; }
        public string? EstatusAcademicoTexto { get; set; }
        public bool Promovido { get; set; }
        public string? PromovidoA { get; set; }
    }
}
