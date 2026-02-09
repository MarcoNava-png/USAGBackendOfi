namespace WebApplication2.Core.Requests.Aspirante
{
    public class InscribirAspiranteRequest
    {
        public int? IdPeriodoAcademico { get; set; }

        public bool ForzarInscripcion { get; set; } = false;

        public string? Observaciones { get; set; }
    }
}
