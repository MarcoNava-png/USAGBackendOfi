using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs.Ticket
{
    public class ActualizarTicketDto
    {
        public string? Titulo { get; set; }
        public string? Descripcion { get; set; }
        public TicketPrioridadEnum? Prioridad { get; set; }
        public TicketCategoriaEnum? Categoria { get; set; }
    }
}
