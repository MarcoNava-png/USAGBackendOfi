namespace WebApplication2.Core.DTOs
{
    public class ProfesorDto
    {
        public int IdProfesor { get; set; }

        public string NoEmpleado { get; set; } = null!;

        public string NombreCompleto { get; set; }

        public string? EmailInstitucional { get; set; }
    }
}
