namespace WebApplication2.Core.DTOs.Convenio
{
    public class AspiranteConvenioDto
    {
        public int IdAspiranteConvenio { get; set; }
        public int IdAspirante { get; set; }
        public string? NombreAspirante { get; set; }
        public int IdConvenio { get; set; }
        public string? ClaveConvenio { get; set; }
        public string? NombreConvenio { get; set; }
        public string? TipoBeneficio { get; set; }
        public decimal? DescuentoPct { get; set; }
        public decimal? Monto { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public string Estatus { get; set; } = null!;
        public string? Evidencia { get; set; }
        public string? AplicaA { get; set; }
        public int? MaxAplicaciones { get; set; }
        public int VecesAplicado { get; set; }
        public bool PuedeAplicarse { get; set; }
    }
}
