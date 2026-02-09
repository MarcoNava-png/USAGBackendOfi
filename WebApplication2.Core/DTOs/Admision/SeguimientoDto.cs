namespace WebApplication2.Core.DTOs.Admision
{
    public class SeguimientoDto
    {
        public AsesorDto? AsesorAsignado { get; set; }
        public string? MedioContacto { get; set; }
        public List<BitacoraSeguimientoDto> Bitacora { get; set; } = new();
    }
}
