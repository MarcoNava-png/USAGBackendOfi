namespace WebApplication2.Core.DTOs.Documentos
{
    public class KardexMateriaDto
    {
        public string ClaveMateria { get; set; } = string.Empty;
        public string NombreMateria { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public decimal? CalificacionFinal { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public string? TipoAcreditacion { get; set; }
    }
}
