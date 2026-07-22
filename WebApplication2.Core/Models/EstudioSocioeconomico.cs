namespace WebApplication2.Core.Models;

public partial class EstudioSocioeconomico : BaseEntity
{
    public int IdEstudioSocioeconomico { get; set; }

    public int IdAspirante { get; set; }

    public int? IdParentescoVivienda { get; set; }

    public string? ConQuienViveOtro { get; set; }

    public int? NumeroPersonasHogar { get; set; }

    public string? PrincipalSostenEconomico { get; set; }

    public int? PersonasAportanIngresos { get; set; }

    public bool? Trabaja { get; set; }

    public string? EmpresaActividad { get; set; }

    public string? HorarioLaboral { get; set; }

    public string? QuienCubreGastos { get; set; }

    public bool? DificultadesEconomicas { get; set; }

    public int? IdServicioMedico { get; set; }

    public bool? PadeceEnfermedad { get; set; }

    public string? PadeceEnfermedadDetalle { get; set; }

    public bool? TieneDiscapacidad { get; set; }

    public string? TieneDiscapacidadDetalle { get; set; }

    public string? EscuelaProcedencia { get; set; }

    public decimal? PromedioNivelAnterior { get; set; }

    public string? AnalistaId { get; set; }

    public DateTime? FechaLlenado { get; set; }

    public bool LlenadoPorAspirante { get; set; }

    public string? Token { get; set; }

    public virtual Aspirante IdAspiranteNavigation { get; set; } = null!;

    public virtual CatParentescoVivienda? IdParentescoViviendaNavigation { get; set; }

    public virtual CatServicioMedico? IdServicioMedicoNavigation { get; set; }

    public virtual ICollection<EstudioServicioVivienda> EstudioServicioVivienda { get; set; } = new List<EstudioServicioVivienda>();

    public virtual ICollection<EstudioRecursoTecnologico> EstudioRecursoTecnologico { get; set; } = new List<EstudioRecursoTecnologico>();
}
