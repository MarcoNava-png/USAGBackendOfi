namespace WebApplication2.Core.Requests.BecaCatalogo
{
    public class CrearBecaCatalogoRequest
    {
        public string Clave { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Descripcion { get; set; }
        public string Tipo { get; set; } = "PORCENTAJE";
        public decimal Valor { get; set; }
        public decimal? TopeMensual { get; set; }
        public int? IdConceptoPago { get; set; }
    }
}
