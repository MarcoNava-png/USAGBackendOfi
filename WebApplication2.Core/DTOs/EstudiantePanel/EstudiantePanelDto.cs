namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class EstudiantePanelDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Curp { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Fotografia { get; set; }
        public bool Activo { get; set; }

        public InformacionAcademicaPanelDto InformacionAcademica { get; set; } = new();

        public ResumenKardexDto ResumenKardex { get; set; } = new();

        public List<BecaAsignadaDto> Becas { get; set; } = new();

        public ResumenRecibosDto ResumenRecibos { get; set; } = new();

        public DocumentosDisponiblesDto Documentos { get; set; } = new();

        public ContactoEmergenciaDto? ContactoEmergencia { get; set; }

        public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;
    }
}
