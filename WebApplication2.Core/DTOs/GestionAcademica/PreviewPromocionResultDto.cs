namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class PreviewPromocionResultDto
    {
        public int IdGrupoOrigen { get; set; }
        public string GrupoOrigen { get; set; } = string.Empty;
        public string CodigoGrupoOrigen { get; set; } = string.Empty;
        public int CuatrimestreOrigen { get; set; }
        public string PlanEstudios { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
        public int? IdGrupoDestino { get; set; }
        public string? GrupoDestino { get; set; }
        public string? CodigoGrupoDestino { get; set; }
        public int CuatrimestreDestino { get; set; }
        public bool GrupoDestinoExiste { get; set; }
        public string PeriodoDestino { get; set; } = string.Empty;
        public int TotalEstudiantes { get; set; }
        public int EstudiantesElegibles { get; set; }
        public int EstudiantesConPagosPendientes { get; set; }
        public decimal TotalSaldoPendiente { get; set; }
        public List<EstudiantePreviewDto> Estudiantes { get; set; } = new();
    }
}
