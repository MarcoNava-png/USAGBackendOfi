namespace WebApplication2.Core.DTOs.Admision
{
    public class DatosPersonalesDto
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? CURP { get; set; }
        public string? RFC { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public int? Edad { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public string? FotoUrl { get; set; }
    }
}
