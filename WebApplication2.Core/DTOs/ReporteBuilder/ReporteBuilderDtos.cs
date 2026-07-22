namespace WebApplication2.Core.DTOs.ReporteBuilder
{
    public class ReporteCampoDto
    {
        public string Clave { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
        public string Tipo { get; set; } = "texto";
        public bool Filtrable { get; set; }
        public string? Catalogo { get; set; }
    }

    public class ReporteDefinicionDto
    {
        public int IdReporteDefinicion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Fuente { get; set; } = string.Empty;
        public List<string> Columnas { get; set; } = new();
        public List<ReporteFiltroDto> Filtros { get; set; } = new();
        public string? OrdenCampo { get; set; }
        public bool OrdenDescendente { get; set; }
        public string? AgruparPor { get; set; }
    }

    public class ReporteFuenteDto
    {
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public List<ReporteCampoDto> Campos { get; set; } = new();
    }

    public class ReporteFiltroDto
    {
        public string Campo { get; set; } = string.Empty;
        public string? Valor { get; set; }
    }

    public class EjecutarReporteRequest
    {
        public string Fuente { get; set; } = string.Empty;
        public List<string> Columnas { get; set; } = new();
        public List<ReporteFiltroDto> Filtros { get; set; } = new();
        public string? OrdenCampo { get; set; }
        public bool OrdenDescendente { get; set; }
        public string? AgruparPor { get; set; }
    }

    public class ReporteSubtotalDto
    {
        public string Grupo { get; set; } = string.Empty;
        public int Conteo { get; set; }
        public Dictionary<string, decimal> Sumas { get; set; } = new();
        public Dictionary<string, decimal> Promedios { get; set; } = new();
    }

    public class ReporteResultadoDto
    {
        public List<ReporteCampoDto> Columnas { get; set; } = new();
        public List<Dictionary<string, object?>> Filas { get; set; } = new();
        public int Total { get; set; }
        public string? AgrupadoPor { get; set; }
        public List<string> CamposNumericos { get; set; } = new();
        public List<ReporteSubtotalDto> Subtotales { get; set; } = new();
        public ReporteSubtotalDto? TotalGeneral { get; set; }
    }
}
