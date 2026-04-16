namespace WebApplication2.Core.Models.Titulacion;

public class CredencialSEP : BaseEntity
{
    public int Id { get; set; }

    public string Usuario { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? EndpointUrl { get; set; }
    public bool EsProduccion { get; set; }

    public int IdConfiguracionIPES { get; set; }
    public virtual ConfiguracionIPES ConfiguracionIPESNavigation { get; set; } = null!;

    public bool Activa { get; set; } = true;
}
