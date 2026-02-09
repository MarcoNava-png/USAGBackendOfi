namespace WebApplication2.Core.Requests.Estudiante
{
    public class ActualizarDatosEstudianteRequest
    {
        public string Nombre { get; set; } = null!;
        public string ApellidoPaterno { get; set; } = null!;
        public string ApellidoMaterno { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
        public string? Curp { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? Direccion { get; set; }
        public string? NombreContactoEmergencia { get; set; }
        public string? TelefonoContactoEmergencia { get; set; }
        public string? ParentescoContactoEmergencia { get; set; }
    }
}
