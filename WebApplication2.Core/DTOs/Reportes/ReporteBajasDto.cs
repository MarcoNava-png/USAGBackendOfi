namespace WebApplication2.Core.DTOs.Reportes
{
    public class ReporteBajasDto
    {
        public string? PlanEstudios { get; set; }
        public string? PeriodoAcademico { get; set; }
        public string? MesFiltro { get; set; }
        public string FechaGeneracion { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        public int TotalBajas { get; set; }
        public int BajasTemporales { get; set; }
        public int BajasDefinitivas { get; set; }
        public int BajasAdministrativas { get; set; }
        public int BajasAcademicas { get; set; }
        public decimal TotalSaldoPendiente { get; set; }
        public List<EstudianteBajaItemDto> Estudiantes { get; set; } = new();
    }

    public class EstudianteBajaItemDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? PlanEstudios { get; set; }
        public string? UltimoGrupo { get; set; }
        public string TipoBaja { get; set; } = string.Empty;
        public string EstadoBaja { get; set; } = string.Empty;
        public string? MotivoBaja { get; set; }
        public DateTime? FechaBaja { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public decimal SaldoPendiente { get; set; }
        public decimal TotalPagado { get; set; }
        public DateTime? UltimoPago { get; set; }
        public decimal? MontoUltimoPago { get; set; }
    }
}
