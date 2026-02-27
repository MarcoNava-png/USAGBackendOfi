namespace WebApplication2.Core.DTOs.TarifaAdmision
{
    public class TarifaAdmisionDetalleDto
    {
        public int IdTarifaAdmisionDetalle { get; set; }
        public int IdConceptoPago { get; set; }
        public string ClaveConcepto { get; set; } = null!;
        public string NombreConcepto { get; set; } = null!;
        public string TipoConcepto { get; set; } = null!;
        public decimal Monto { get; set; }
        public bool EsAplicable { get; set; }
        public string? Notas { get; set; }
        public int Orden { get; set; }
    }
}
