using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebApplication2.Core.Requests.Auth
{
    public class CreateUserRequest : LoginRequest
    {
        [Required(ErrorMessage = "Los nombres son requeridos")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son requeridos")]
        public string Apellidos { get; set; } = string.Empty;

        [Phone(ErrorMessage = "El teléfono no es válido")]
        public string? Telefono { get; set; }

        [DefaultValue(null)]
        public string? Biografia { get; set; }

        [DefaultValue(null)]
        public string? PhotoUrl { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
