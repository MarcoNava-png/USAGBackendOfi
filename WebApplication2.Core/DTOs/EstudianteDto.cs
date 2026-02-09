namespace WebApplication2.Core.DTOs
{
    public class EstudianteDto
    {
        public int IdEstudiante { get; set; }

        public string Matricula { get; set; } = null!;

        public string NombreCompleto { get; set; }

        public string Telefono { get; set; }
        public string PlanEstudios { get; set; }
        public string email { get; set; }
        public int? IdPlanActual { get; set; }
    }
}
