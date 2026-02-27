using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models
{
    public class TicketSoporte : BaseEntity
    {
        public int IdTicket { get; set; }

        public string Folio { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public TicketPrioridadEnum Prioridad { get; set; } = TicketPrioridadEnum.Baja;

        public TicketEstatusEnum Estatus { get; set; } = TicketEstatusEnum.Abierto;

        public TicketCategoriaEnum Categoria { get; set; } = TicketCategoriaEnum.General;

        public string UsuarioCreadorId { get; set; } = string.Empty;

        public string NombreCreador { get; set; } = string.Empty;

        public string? UsuarioAsignadoId { get; set; }

        public string? NombreAsignado { get; set; }

        public string? ArchivoAdjuntoUrl { get; set; }

        public string? ArchivoAdjuntoNombre { get; set; }

        public DateTime? FechaCierre { get; set; }

        public virtual ICollection<TicketComentario> Comentarios { get; set; } = new List<TicketComentario>();
    }
}
