namespace WebApplication2.Core.DTOs.Admision
{
    public class DatosContactoDto
    {
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public DireccionDto? Direccion { get; set; }
        public string? NombreContactoEmergencia { get; set; }
        public string? TelefonoContactoEmergencia { get; set; }
        public string? ParentescoContactoEmergencia { get; set; }
    }
}
