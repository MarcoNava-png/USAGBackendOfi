namespace WebApplication2.Core.DTOs
{
    public class PlanDocumentoRequisitoDto
    {
        public int IdDocumentoRequisito { get; set; }
        public string Clave { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public bool EsObligatorio { get; set; }
        public int Orden { get; set; }
    }

    public class ActualizarDocumentosPlanRequest
    {
        public List<PlanDocumentoRequisitoItemDto> Documentos { get; set; } = new();
    }

    public class PlanDocumentoRequisitoItemDto
    {
        public int IdDocumentoRequisito { get; set; }
        public bool EsObligatorio { get; set; }
    }

    public class DocumentoRequisitoDisponibleDto
    {
        public int IdDocumentoRequisito { get; set; }
        public string Clave { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public bool EsObligatorio { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }
}
