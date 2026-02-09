namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class TipoDocumentoDisponibleDto
    {
        public int IdTipoDocumento { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int DiasVigencia { get; set; }
        public bool RequierePago { get; set; }
        public bool TieneSolicitudPendiente { get; set; }
        public bool TieneDocumentoVigente { get; set; }
    }
}
