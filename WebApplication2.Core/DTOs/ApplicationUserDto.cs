namespace WebApplication2.Core.DTOs
{
    public class ApplicationUserDto
    {
        public string Id { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string Biografia { get; set; }
        public string PhotoUrl { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
}
