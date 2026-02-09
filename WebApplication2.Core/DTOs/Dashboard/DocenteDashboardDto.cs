namespace WebApplication2.Core.DTOs.Dashboard
{
    public class DocenteDashboardDto
    {
        public List<ClaseHoyDto> ClasesDeHoy { get; set; } = new();
        public List<ClaseHoyDto> ProximasClases { get; set; } = new();

        public int AsistenciasPorPasar { get; set; }
        public int EvaluacionesPendientes { get; set; }

        public List<GrupoDocenteDto> MisGrupos { get; set; } = new();

        public List<FechaImportanteDto> FechasCierreCalificaciones { get; set; } = new();

        public List<AnuncioDto> Anuncios { get; set; } = new();

        public List<AlertaDto> Alertas { get; set; } = new();
    }
}
