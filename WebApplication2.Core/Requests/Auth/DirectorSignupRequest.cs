namespace WebApplication2.Core.Requests.Auth
{
    public class DirectorSignupRequest : PersonaSignupRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
