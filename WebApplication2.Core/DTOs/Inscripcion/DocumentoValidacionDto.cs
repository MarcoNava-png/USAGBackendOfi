namespace WebApplication2.Core.DTOs.Inscripcion
{
    public class DocumentoValidacionDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public bool EsObligatorio { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public bool Cumple { get; set; }
    }
}
