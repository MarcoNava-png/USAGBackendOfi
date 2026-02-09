namespace WebApplication2.Core.DTOs.Convenio
{
    public class ConvenioDisponibleDto
    {
        public int IdConvenio { get; set; }
        public string ClaveConvenio { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string TipoBeneficio { get; set; } = null!;
        public decimal? DescuentoPct { get; set; }
        public decimal? Monto { get; set; }
        public string DescripcionBeneficio { get; set; } = null!;
        public string AplicaA { get; set; } = "TODOS";
        public int? MaxAplicaciones { get; set; }
    }
}
