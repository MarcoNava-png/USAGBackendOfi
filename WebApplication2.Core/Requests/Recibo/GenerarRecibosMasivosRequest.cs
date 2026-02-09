namespace WebApplication2.Core.Requests.Recibo
{
    public class GenerarRecibosMasivosRequest
    {
        public int IdPlantillaCobro { get; set; }
        public int IdPeriodoAcademico { get; set; }
        public bool SoloSimular { get; set; } = false;
        public List<int>? IdEstudiantes { get; set; }
    }
}
