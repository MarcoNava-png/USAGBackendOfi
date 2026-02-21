namespace WebApplication2.Core.Requests.DocumentoRequisito
{
    public class DocumentoRequisitoUpdateRequest : DocumentoRequisitoRequest
    {
        public int IdDocumentoRequisito { get; set; }
        public bool Activo { get; set; } = true;
    }
}
