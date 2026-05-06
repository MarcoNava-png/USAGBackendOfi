namespace WebApplication2.Core.DTOs.PortalAlumno;

public class MisDocumentosPendientesDto
{
    public int TotalPendientes { get; set; }
    public int ConProrrogaVigente { get; set; }
    public int ConProrrogaVencida { get; set; }
    public int SinProrroga { get; set; }
    public DateTime? ProximoVencimiento { get; set; }
    public List<MiDocumentoPendienteDto> Documentos { get; set; } = new();
}

public class MiDocumentoPendienteDto
{
    public long IdAspiranteDocumento { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estatus { get; set; } = string.Empty;
    public bool EsObligatorio { get; set; }
    public DateTime? FechaProrroga { get; set; }
    public string? MotivoProrroga { get; set; }
    public bool TieneProrrogaVigente { get; set; }
    public bool ProrrogaVencida { get; set; }
    public int? DiasRestantes { get; set; }
}
