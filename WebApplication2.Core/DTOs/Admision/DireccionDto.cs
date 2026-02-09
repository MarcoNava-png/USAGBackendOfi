namespace WebApplication2.Core.DTOs.Admision
{
    public class DireccionDto
    {
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? NumeroInterior { get; set; }
        public string? Colonia { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Municipio { get; set; }
        public string? Estado { get; set; }
        public string DireccionCompleta { get; set; } = string.Empty;
    }
}
