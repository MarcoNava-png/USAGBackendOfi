namespace WebApplication2.Core.DTOs.Recibo
{
    public class ReciboEstudianteResumen
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public int RecibosGenerados { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal DescuentoBecas { get; set; }
        public decimal SaldoFinal { get; set; }
    }
}
