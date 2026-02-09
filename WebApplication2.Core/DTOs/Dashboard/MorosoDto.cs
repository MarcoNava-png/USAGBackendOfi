namespace WebApplication2.Core.DTOs.Dashboard
{
    public class MorosoDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public decimal MontoAdeudado { get; set; }
        public int DiasVencido { get; set; }
    }
}
