using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.DTOs.Importacion
{
    public class ImportarProfesorDto
    {
        [Required(ErrorMessage = "La clave/numero de empleado es requerida")]
        public string NoEmpleado { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es requerido")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = string.Empty;

        public string? Genero { get; set; }

        public string? Rfc { get; set; }

        public string? Curp { get; set; }

        public string? FechaNacimiento { get; set; }

        public string? Domicilio { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public string? EstadoFederativo { get; set; }

        public string? CedulaProfesional { get; set; }
    }
}
