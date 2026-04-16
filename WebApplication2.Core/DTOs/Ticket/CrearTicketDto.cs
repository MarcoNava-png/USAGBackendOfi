using WebApplication2.Core.Enums;

namespace WebApplication2.Core.DTOs.Ticket
{
    public class CrearTicketDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public TicketPrioridadEnum Prioridad { get; set; } = TicketPrioridadEnum.Baja;
        public TicketCategoriaEnum Categoria { get; set; } = TicketCategoriaEnum.General;
        public string? AreaDestino { get; set; }
    }
}
