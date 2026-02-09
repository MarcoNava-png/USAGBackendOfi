namespace WebApplication2.Core.DTOs
{
    public class NotificacionUsuarioDto
    {
        public long IdNotificacion { get; set; }
        public string Titulo { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public string Tipo { get; set; } = null!;
        public string? Modulo { get; set; }
        public string? UrlAccion { get; set; }
        public bool Leida { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLectura { get; set; }
    }
}
