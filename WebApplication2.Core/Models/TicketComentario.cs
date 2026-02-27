namespace WebApplication2.Core.Models
{
    public class TicketComentario : BaseEntity
    {
        public int IdComentario { get; set; }

        public int IdTicket { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

        public string NombreUsuario { get; set; } = string.Empty;

        public string Contenido { get; set; } = string.Empty;

        public bool EsAdmin { get; set; }

        public string? ArchivoAdjuntoUrl { get; set; }

        public string? ArchivoAdjuntoNombre { get; set; }

        public virtual TicketSoporte Ticket { get; set; } = null!;
    }
}
