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
        public string? Genero { get; set; }
        public string? Direccion { get; set; }
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? NumeroInterior { get; set; }
        public string? Colonia { get; set; }
        public string? CodigoPostalStr { get; set; }
        public string? MunicipioStr { get; set; }
        public string? EstadoStr { get; set; }
        public bool Activo { get; set; }
        public int EstatusAcademico { get; set; }
        public string? EstatusAcademicoTexto { get; set; }
        public bool TienePreinscripcionPendiente { get; set; }
        public string? PeriodoPreinscripcion { get; set; }
        public int? TipoBaja { get; set; }
        public int? EstadoBaja { get; set; }
        public string? MotivoBaja { get; set; }
        public DateTime? FechaBaja { get; set; }

        public InformacionAcademicaPanelDto InformacionAcademica { get; set; } = new();

        public ResumenKardexDto ResumenKardex { get; set; } = new();

        public List<BecaAsignadaDto> Becas { get; set; } = new();

        public ResumenRecibosDto ResumenRecibos { get; set; } = new();

        public DocumentosDisponiblesDto Documentos { get; set; } = new();

        public ContactoEmergenciaDto? ContactoEmergencia { get; set; }

        public DateTime FechaConsulta { get; set; } = DateTime.UtcNow;
    }
}
