namespace WebApplication2.Core.DTOs
{
    public class BitacoraAccionDto
    {
        public long IdBitacora { get; set; }
        public string UsuarioId { get; set; } = null!;
        public string NombreUsuario { get; set; } = null!;
        public string Accion { get; set; } = null!;
        public string Modulo { get; set; } = null!;
        public string Entidad { get; set; } = null!;
        public string? EntidadId { get; set; }
        public string? Descripcion { get; set; }
        public string? DatosAnteriores { get; set; }
        public string? DatosNuevos { get; set; }
        public string? IpAddress { get; set; }
        public DateTime FechaUtc { get; set; }
    }

    public class BitacoraAccionFiltroDto
    {
        public string? Modulo { get; set; }
        public string? Usuario { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Busqueda { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
