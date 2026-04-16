using WebApplication2.Core.Enums;

namespace WebApplication2.Core.Models.Titulacion;

public class CertificadoElectronico : BaseEntity
{
    public int Id { get; set; }

    public string? FolioControl { get; set; }
    public TipoTitulacionEnum TipoTitulacion { get; set; }
    public EstatusCertificadoEnum Estatus { get; set; } = EstatusCertificadoEnum.Registro;

    public int? IdEstudiante { get; set; }
    public virtual Estudiante? EstudianteNavigation { get; set; }

    public int? IdPersona { get; set; }
    public virtual Persona? PersonaNavigation { get; set; }

    public string NumeroControl { get; set; } = null!;
    public string? Curp { get; set; }
    public string Nombre { get; set; } = null!;
    public string PrimerApellido { get; set; } = null!;
    public string? SegundoApellido { get; set; }
    public int IdGenero { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string? FotoHash { get; set; }
    public string? FirmaAutografaHash { get; set; }

    public string IdCarreraSEP { get; set; } = null!;
    public string? ClaveCarrera { get; set; }
    public string? NombreCarrera { get; set; }
    public string IdTipoPeriodo { get; set; } = null!;
    public string? TipoPeriodo { get; set; }
    public string ClavePlan { get; set; } = null!;

    public string NumeroRvoe { get; set; } = null!;
    public DateTime FechaExpedicionRvoe { get; set; }

    public string IdTipoCertificacion { get; set; } = null!;
    public string? TipoCertificacion { get; set; }
    public DateTime FechaExpedicion { get; set; }
    public string IdLugarExpedicion { get; set; } = null!;
    public string? LugarExpedicion { get; set; }

    public int TotalAsignaturas { get; set; }
    public int AsignaturasAsignadas { get; set; }
    public string? Promedio { get; set; }
    public decimal? CreditosObtenidos { get; set; }
    public decimal? TotalCreditos { get; set; }
    public int? NumeroCiclos { get; set; }

    public int IdConfiguracionIPES { get; set; }
    public virtual ConfiguracionIPES ConfiguracionIPESNavigation { get; set; } = null!;

    public int? IdResponsableFirma { get; set; }
    public virtual ResponsableFirma? ResponsableFirmaNavigation { get; set; }

    public string? SelloDigital { get; set; }
    public string? CadenaOriginal { get; set; }
    public string? XmlGenerado { get; set; }

    public int? NumeroLoteSEP { get; set; }
    public int? EstatusLoteSEP { get; set; }
    public string? FolioControlSEP { get; set; }
    public string? MensajeSEP { get; set; }
    public DateTime? FechaEnvioSEP { get; set; }
    public DateTime? FechaRespuestaSEP { get; set; }

    public virtual ICollection<CertificadoAsignatura> Asignaturas { get; set; } = new List<CertificadoAsignatura>();
}
