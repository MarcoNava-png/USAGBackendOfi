namespace WebApplication2.Core.Requests.Catalogos
{
    public class CrearPeriodicidadRequest
    {
        public string DescPeriodicidad { get; set; } = null!;

        public byte PeriodosPorAnio { get; set; }

        public byte MesesPorPeriodo { get; set; }
    }
}
