namespace WebApplication2.Core.DTOs.Convenio
{
    public class CrearConvenioDto
    {
        public string ClaveConvenio { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string TipoBeneficio { get; set; } = null!;
        public decimal? DescuentoPct { get; set; }
        public decimal? Monto { get; set; }
        public DateOnly? VigenteDesde { get; set; }
        public DateOnly? VigenteHasta { get; set; }
        public string AplicaA { get; set; } = "TODOS";
        public int? MaxAplicaciones { get; set; }
        public bool Activo { get; set; } = true;
        public List<CrearConvenioAlcanceDto> Alcances { get; set; } = new();
    }
}
