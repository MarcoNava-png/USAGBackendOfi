using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Requests.Auth
{
    public class UserSignupRequest
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> Roles { get; set; }
        public string Name { get; set; }
        public string PaternalSurname { get; set; }
        public string MaternalSurname { get; set; }
    }
}
