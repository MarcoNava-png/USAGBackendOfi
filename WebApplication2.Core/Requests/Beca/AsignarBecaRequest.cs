namespace WebApplication2.Core.Requests.Beca
{
    public class AsignarBecaRequest
    {
        public int IdEstudiante { get; set; }
        public int? IdConceptoPago { get; set; }
        public string Tipo { get; set; } = "PORCENTAJE";
        public decimal Valor { get; set; }
        public DateOnly VigenciaDesde { get; set; }
        public DateOnly? VigenciaHasta { get; set; }
        public decimal? TopeMensual { get; set; }
        public string? Observaciones { get; set; }
    }
}
