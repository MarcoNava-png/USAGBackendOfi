namespace WebApplication2.Core.DTOs.Dashboard
{
    public class AlumnoDashboardDto
    {
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Programa { get; set; } = string.Empty;
        public int Cuatrimestre { get; set; }

        public List<ClaseAlumnoDto> HorarioHoy { get; set; } = new();
        public List<ClaseAlumnoDto> ProximasClases { get; set; } = new();

        public List<CalificacionRecienteDto> CalificacionesRecientes { get; set; } = new();
        public decimal PromedioActual { get; set; }

        public bool TieneDeuda { get; set; }
        public decimal? MontoDeuda { get; set; }
        public DateTime? ProximoVencimiento { get; set; }

        public decimal PorcentajeAsistencia { get; set; }

        public List<AnuncioDto> Anuncios { get; set; } = new();

        public List<TramiteDisponibleDto> TramitesDisponibles { get; set; } = new();

        public List<AlertaDto> Alertas { get; set; } = new();
    }
}
