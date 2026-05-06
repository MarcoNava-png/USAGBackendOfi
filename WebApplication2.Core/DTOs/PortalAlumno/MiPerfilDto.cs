namespace WebApplication2.Core.DTOs.PortalAlumno
{
    public class MiPerfilDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Nombre { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public string? Curp { get; set; }
        public string? Rfc { get; set; }
        public DateOnly? FechaNacimiento { get; set; }
        public string? Genero { get; set; }
        public string? EstadoCivil { get; set; }
        public string? Nacionalidad { get; set; }
        public string? FotoUrl { get; set; }

        public MiDireccionDto? Direccion { get; set; }
        public MiContactoEmergenciaDto? ContactoEmergencia { get; set; }

        public string? PlanEstudios { get; set; }
        public string? ClavePlanEstudios { get; set; }
        public string? Campus { get; set; }
        public DateOnly? FechaIngreso { get; set; }
    }

    public class MiDireccionDto
    {
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? NumeroInterior { get; set; }
        public string? Colonia { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
    }

    public class MiContactoEmergenciaDto
    {
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Parentesco { get; set; }
    }
}
