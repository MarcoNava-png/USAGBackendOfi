namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class NotificacionDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = null!;
        public string Titulo { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public string? TenantCodigo { get; set; }
        public string? TenantNombre { get; set; }
        public int? IdTenant { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Leida { get; set; }
        public DateTime? FechaLectura { get; set; }
        public string Prioridad { get; set; } = "Normal";
        public string? AccionUrl { get; set; }
    }
}
