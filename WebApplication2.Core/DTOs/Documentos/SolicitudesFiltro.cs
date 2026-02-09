namespace WebApplication2.Core.DTOs.Documentos
{
    public class SolicitudesFiltro
    {
        public int? IdEstudiante { get; set; }
        public int? IdTipoDocumento { get; set; }
        public string? Estatus { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Busqueda { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 20;
    }
}
