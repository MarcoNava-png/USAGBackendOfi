namespace WebApplication2.Core.Requests.Auth
{
    public class CoordinadorSignupRequest : PersonaSignupRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
