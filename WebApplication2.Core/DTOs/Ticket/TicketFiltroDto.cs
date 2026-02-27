using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs.Ticket
{
    public class TicketFiltroDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public TicketEstatusEnum? Estatus { get; set; }
        public TicketPrioridadEnum? Prioridad { get; set; }
        public TicketCategoriaEnum? Categoria { get; set; }
        public string? Busqueda { get; set; }
    }
}
