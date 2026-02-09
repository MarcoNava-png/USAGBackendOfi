namespace WebApplication2.Core.DTOs
{
    public class PersonaDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Estatus { get; set; }
        public string PersonaGenero { get; set; }
    }
}
