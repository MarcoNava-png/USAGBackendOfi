namespace WebApplication2.Core.DTOs.Documentos
{
    public class AsignarProrrogaRequest
    {
        public long IdAspiranteDocumento { get; set; }
        public DateTime FechaProrroga { get; set; }
        public string? Motivo { get; set; }
    }

    public class ProrrogaGlobalRequest
    {
        public int IdAspirante { get; set; }
        public DateTime FechaProrroga { get; set; }
        public string? Motivo { get; set; }
    }

    public class DocumentacionAspiranteResumenDto
    {
        public int IdAspirante { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Matricula { get; set; }
        public string PlanEstudios { get; set; } = string.Empty;
        public int TotalDocumentos { get; set; }
        public int DocumentosCompletos { get; set; }
        public int DocumentosPendientes { get; set; }
        public int DocumentosConProrroga { get; set; }
        public int ProrrogasVencidas { get; set; }
        public string EstatusGeneral { get; set; } = string.Empty;
        public List<AspiranteDocumentoDetalleDto> Documentos { get; set; } = new();
    }

    public class AspiranteDocumentoDetalleDto
    {
        public long IdAspiranteDocumento { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool EsObligatorio { get; set; }
        public string Estatus { get; set; } = string.Empty;
        public DateTime? FechaSubida { get; set; }
        public DateTime? FechaProrroga { get; set; }
        public string? MotivoProrroga { get; set; }
        public bool ProrrogaVencida { get; set; }
        public string? UrlArchivo { get; set; }
        public string? Notas { get; set; }
    }
}
