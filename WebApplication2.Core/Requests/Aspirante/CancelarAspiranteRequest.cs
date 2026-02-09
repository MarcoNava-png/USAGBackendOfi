using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Requests.Aspirante
{
    public class CancelarAspiranteRequest
    {
        [Required(ErrorMessage = "Debe proporcionar un motivo para la cancelación")]
        [MinLength(10, ErrorMessage = "El motivo debe tener al menos 10 caracteres")]
        public string Motivo { get; set; } = string.Empty;
    }
}
