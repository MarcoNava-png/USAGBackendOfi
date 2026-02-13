namespace WebApplication2.Core.DTOs
{
    public class ProfesorDto
    {
        public int IdProfesor { get; set; }

        public string NoEmpleado { get; set; } = null!;

        public string NombreCompleto { get; set; }

        public string? EmailInstitucional { get; set; }

        // Persona fields
        public string? Nombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string? Curp { get; set; }
        public string? Rfc { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public int? GeneroId { get; set; }
        public int? IdEstadoCivil { get; set; }
        public int? CampusId { get; set; }

        // Direccion fields
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? NumeroInterior { get; set; }
        public int? CodigoPostalId { get; set; }
    }
}
