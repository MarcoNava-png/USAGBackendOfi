namespace WebApplication2.Core.Models.Titulacion;

public class CertificadoAsignatura : BaseEntity
{
    public int Id { get; set; }

    public int IdCertificadoElectronico { get; set; }
    public virtual CertificadoElectronico CertificadoElectronicoNavigation { get; set; } = null!;

    public int IdAsignatura { get; set; }
    public string? ClaveAsignatura { get; set; }
    public string Nombre { get; set; } = null!;
    public string Ciclo { get; set; } = null!;
    public string Calificacion { get; set; } = null!;
    public decimal? Creditos { get; set; }
    public int? IdTipoAsignatura { get; set; }
    public string? TipoAsignatura { get; set; }
    public int? IdObservaciones { get; set; }
    public string? Observaciones { get; set; }
}
