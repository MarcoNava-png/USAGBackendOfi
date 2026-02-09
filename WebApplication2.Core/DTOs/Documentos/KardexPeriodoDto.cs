namespace WebApplication2.Core.DTOs.Documentos
{
    public class KardexPeriodoDto
    {
        public string Periodo { get; set; } = string.Empty;
        public string Ciclo { get; set; } = string.Empty;
        public List<KardexMateriaDto> Materias { get; set; } = new();
        public decimal PromedioPeriodo { get; set; }
        public int CreditosPeriodo { get; set; }
    }
}
