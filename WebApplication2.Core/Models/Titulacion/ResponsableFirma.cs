namespace WebApplication2.Core.Models.Titulacion;

public class ResponsableFirma : BaseEntity
{
    public int Id { get; set; }

    public string Curp { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string PrimerApellido { get; set; } = null!;
    public string? SegundoApellido { get; set; }
    public string IdCargo { get; set; } = null!;
    public string? Cargo { get; set; }

    public string? RutaCertificadoCer { get; set; }
    public string? RutaLlavePrivadaKey { get; set; }
    public string? PasswordLlavePrivada { get; set; }
    public string? NoCertificadoResponsable { get; set; }

    public int IdConfiguracionIPES { get; set; }
    public virtual ConfiguracionIPES ConfiguracionIPESNavigation { get; set; } = null!;

    public bool Activo { get; set; } = true;
    public DateTime? VigenciaInicio { get; set; }
    public DateTime? VigenciaFin { get; set; }
}
