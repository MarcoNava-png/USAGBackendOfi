using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Core.Requests.Auth
{
    public class PersonaSignupRequest
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        public string ApellidoPaterno { get; set; }

        [Required]
        public string ApellidoMaterno { get; set; }

        [Required]
        public DateOnly FechaNacimiento { get; set; }

        [Required]
        public int GeneroId { get; set; }

        [Required]
        public string Correo { get; set; }

        [Required]
        public string Telefono { get; set; }

        public string? CURP { get; set; }

        public string? Calle { get; set; }

        public string? NumeroExterior { get; set; }

        public string? NumeroInterior { get; set; }

        public int? CodigoPostalId { get; set; }

        public int? IdEstadoCivil { get; set; }
    }
}
