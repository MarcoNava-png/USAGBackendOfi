namespace WebApplication2.Core.Requests.Beca
{
    public class ActualizarBecaRequest
    {
        public int? IdPeriodoAcademico { get; set; }
        public DateOnly? VigenciaDesde { get; set; }
        public DateOnly? VigenciaHasta { get; set; }
        public string? Observaciones { get; set; }
        public bool? Activo { get; set; }
    }
}
