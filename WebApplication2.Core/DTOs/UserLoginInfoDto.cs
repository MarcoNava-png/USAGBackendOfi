namespace WebApplication2.Core.DTOs
{
    public class UserLoginInfoDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string Biografia { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string PhotoUrl { get; set; }
    }
}
