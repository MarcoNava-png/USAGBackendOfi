namespace WebApplication2.Core.Models;

public partial class Aspirante : BaseEntity
{
    public int IdAspirante { get; set; }

    public int? IdPersona { get; set; }

    public int IdAspiranteEstatus { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int IdPlan { get; set; }

    public int IdMedioContacto { get; set; }

    public string? IdAtendidoPorUsuario { get; set; }

    public string? Observaciones { get; set; }

    public int? TurnoId { get; set; }

    public int? CuatrimestreInteres { get; set; }

    public string? InstitucionProcedencia { get; set; }

    public int? IdModalidad { get; set; }

    public int? IdPeriodoAcademico { get; set; }

    public bool? RecorridoPlantel { get; set; }

    public bool? Trabaja { get; set; }

    public string? NombreEmpresa { get; set; }

    public string? DomicilioEmpresa { get; set; }

    public string? PuestoEmpresa { get; set; }

    public string? QuienCubreGastos { get; set; }

    public virtual ICollection<AspiranteConvenio> AspiranteConvenio { get; set; } = new List<AspiranteConvenio>();

    public virtual AspiranteEstatus IdAspiranteEstatusNavigation { get; set; } = null!;


    public virtual MedioContacto IdMedioContactoNavigation { get; set; } = null!;

    public virtual Persona? IdPersonaNavigation { get; set; }

    public virtual PlanEstudios IdPlanNavigation { get; set; } = null!;

    public virtual Turno Turno { get; set; }

    public virtual Modalidad? IdModalidadNavigation { get; set; }

    public virtual PeriodoAcademico? IdPeriodoAcademicoNavigation { get; set; }

    public ICollection<AspiranteDocumento> Documentos { get; set; } = [];
}
