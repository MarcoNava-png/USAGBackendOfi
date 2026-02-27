namespace WebApplication2.Core.Models
{
    public class TarifaAdmisionDetalle : BaseEntity
    {
        public int IdTarifaAdmisionDetalle { get; set; }
        public int IdTarifaAdmision { get; set; }
        public int IdConceptoPago { get; set; }
        public decimal Monto { get; set; }
        public bool EsAplicable { get; set; } = true;
        public string? Notas { get; set; }
        public int Orden { get; set; }

        public virtual TarifaAdmision IdTarifaAdmisionNavigation { get; set; } = null!;
        public virtual ConceptoPago IdConceptoPagoNavigation { get; set; } = null!;
    }
}
