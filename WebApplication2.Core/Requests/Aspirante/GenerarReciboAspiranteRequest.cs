namespace WebApplication2.Core.Requests.Aspirante
{
    public class GenerarReciboAspiranteRequest
    {
        public decimal Monto { get; set; }
        public string Concepto { get; set; } = "Cuota de Inscripción";
        public int DiasVencimiento { get; set; } = 7;
        public int? IdConceptoPago { get; set; }
    }
}
