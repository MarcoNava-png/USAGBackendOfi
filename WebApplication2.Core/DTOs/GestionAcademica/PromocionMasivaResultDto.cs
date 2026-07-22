namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class PromocionMasivaResultDto
    {
        public int TotalPromovidos { get; set; }
        public int TotalEgresados { get; set; }
        public int TotalErrores { get; set; }
        public int GruposCreados { get; set; }
        public string Mensaje { get; set; } = string.Empty;

        public List<PromocionMasivaItemDto> Promovidos { get; set; } = new();
        public List<PromocionMasivaItemDto> Egresados { get; set; } = new();
        public List<PromocionMasivaItemDto> Errores { get; set; } = new();
    }

    public class PromocionMasivaItemDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Campus { get; set; } = string.Empty;
        public string PlanEstudios { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public int Cuatrimestre { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public string? Detalle { get; set; }
        public bool TieneAdeudo { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}
