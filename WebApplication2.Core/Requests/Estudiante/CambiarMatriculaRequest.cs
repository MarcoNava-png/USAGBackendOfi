using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Requests.Estudiante
{
    public class CambiarMatriculaRequest
    {
        [Required]
        [RegularExpression(@"^[A-Z]{1,3}\d{5}$")]
        public string NuevaMatricula { get; set; } = null!;
    }
}
