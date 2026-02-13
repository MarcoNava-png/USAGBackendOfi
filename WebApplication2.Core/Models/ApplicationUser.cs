using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Biografia { get; set; }
        public string? PhotoUrl { get; set; }
        public bool MustChangePassword { get; set; }
    }
}
