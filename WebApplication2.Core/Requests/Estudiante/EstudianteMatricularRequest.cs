using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Requests.Estudiante
{
    public class EstudianteMatricularRequest
    {
        [Required]
        public int IdEstudiante { get; set; }

        [Required]
        public string Matricula { get; set; }
    }
}
