namespace WebApplication2.Core.DTOs.Ticket
{
    public class TicketEstadisticasDto
    {
        public int TotalAbiertos { get; set; }
        public int TotalEnProgreso { get; set; }
        public int TotalResueltos { get; set; }
        public int TotalCerrados { get; set; }
        public int Total { get; set; }
    }
}
