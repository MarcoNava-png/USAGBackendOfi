namespace WebApplication2.Core.Requests.Auth
{
    public class UpdateUserProfileRequest
    {
        public string Email { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string Biografia { get; set; }
        public List<string>? Roles { get; set; }
    }
}
