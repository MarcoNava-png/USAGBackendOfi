using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.DTOs.Importacion
{
    public class ImportarEstudianteDto
    {
        public string? Ciclo { get; set; }

        [Required(ErrorMessage = "El campus es requerido")]
        public string Campus { get; set; } = string.Empty;

        [Required(ErrorMessage = "El curso/carrera es requerido")]
        public string Curso { get; set; } = string.Empty;

        public string? Periodo { get; set; }

        public string? Grupo { get; set; }

        [Required(ErrorMessage = "La matrícula es requerida")]
        public string Matricula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es requerido")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = string.Empty;

        public string? Curp { get; set; }

        public string? FormaPago { get; set; }

        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string? Email { get; set; }

        public string? FechaNacimiento { get; set; }

        public string? FechaInscripcion { get; set; }

        public string? Domicilio { get; set; }

        public string? Colonia { get; set; }

        public string? Celular { get; set; }

        public string? Genero { get; set; }
    }
}
