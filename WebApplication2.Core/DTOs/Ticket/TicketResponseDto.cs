using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs.Ticket
{
    public class TicketResponseDto
    {
        public int IdTicket { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public TicketPrioridadEnum Prioridad { get; set; }
        public string PrioridadNombre { get; set; } = string.Empty;
        public TicketEstatusEnum Estatus { get; set; }
        public string EstatusNombre { get; set; } = string.Empty;
        public TicketCategoriaEnum Categoria { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public string UsuarioCreadorId { get; set; } = string.Empty;
        public string NombreCreador { get; set; } = string.Empty;
        public string? UsuarioAsignadoId { get; set; }
        public string? NombreAsignado { get; set; }
        public string? ArchivoAdjuntoUrl { get; set; }
        public string? ArchivoAdjuntoNombre { get; set; }
        public DateTime? FechaCierre { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<TicketComentarioDto> Comentarios { get; set; } = new();
    }
}
