namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class EstudianteListaDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? PlanEstudios { get; set; }
        public string? Grupo { get; set; }
        public decimal? PromedioGeneral { get; set; }
        public decimal? Adeudo { get; set; }
        public bool TieneBeca { get; set; }
        public bool Activo { get; set; }
        public string? Fotografia { get; set; }
    }
}
