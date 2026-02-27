namespace WebApplication2.Core.DTOs.Ticket
{
    public class TicketComentarioDto
    {
        public int IdComentario { get; set; }
        public string UsuarioId { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public bool EsAdmin { get; set; }
        public string? ArchivoAdjuntoUrl { get; set; }
        public string? ArchivoAdjuntoNombre { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
