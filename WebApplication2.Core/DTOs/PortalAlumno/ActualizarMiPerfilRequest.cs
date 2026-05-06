namespace WebApplication2.Core.DTOs.PortalAlumno
{
    public class ActualizarMiPerfilRequest
    {
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? Email { get; set; }
        public MiDireccionDto? Direccion { get; set; }
        public MiContactoEmergenciaDto? ContactoEmergencia { get; set; }
    }
}
