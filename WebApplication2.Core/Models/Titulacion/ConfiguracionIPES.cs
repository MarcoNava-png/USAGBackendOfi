namespace WebApplication2.Core.Models.Titulacion;

public class ConfiguracionIPES : BaseEntity
{
    public int Id { get; set; }

    public string IdNombreInstitucion { get; set; } = null!;
    public string? NombreInstitucion { get; set; }
    public string IdCampusSEP { get; set; } = null!;
    public string? CampusSEP { get; set; }
    public string IdEntidadFederativa { get; set; } = null!;
    public string? EntidadFederativa { get; set; }

    public int IdCampus { get; set; }
    public virtual Campus CampusNavigation { get; set; } = null!;

    public bool Activa { get; set; } = true;
}
