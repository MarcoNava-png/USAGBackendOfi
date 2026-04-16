namespace WebApplication2.Core.Models;

public class SeguimientoEgresado : BaseEntity
{
    public int IdSeguimientoEgresado { get; set; }
    public int? IdEstudiante { get; set; }
    public string? Matricula { get; set; }
    public string NombreCompleto { get; set; } = null!;
    public string ProgramaAcademico { get; set; } = null!;
    public string? Expediente { get; set; }
    public string? PagoTitulacion { get; set; }
    public string? LiberacionServicioSocial { get; set; }
    public DateTime? FechaSolicitudTitulacion { get; set; }
    public string? EstatusTitulacion { get; set; }
    public string? EstatusCertificado { get; set; }
    public string? EstatusTituloElectronico { get; set; }
    public string? EstatusTituloFisico { get; set; }
    public string? PagoCedula { get; set; }
    public string? TramiteCedula { get; set; }
    public string? Observaciones { get; set; }
    public int NumeroProgramaAcademico { get; set; } = 1;

    public virtual Estudiante? EstudianteNavigation { get; set; }
}
