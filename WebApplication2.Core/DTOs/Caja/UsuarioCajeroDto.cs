namespace WebApplication2.Core.DTOs.Caja
{
    public class UsuarioCajeroDto
    {
        public string IdUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int TotalCobros { get; set; }
        public DateTime? UltimoCobro { get; set; }
    }
}
