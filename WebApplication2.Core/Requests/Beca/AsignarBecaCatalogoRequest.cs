namespace WebApplication2.Core.Requests.Beca
{
    public class AsignarBecaCatalogoRequest
    {
        public int IdEstudiante { get; set; }
        public int IdBeca { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public DateOnly VigenciaDesde { get; set; }
        public DateOnly? VigenciaHasta { get; set; }
        public string? Observaciones { get; set; }
    }
}
