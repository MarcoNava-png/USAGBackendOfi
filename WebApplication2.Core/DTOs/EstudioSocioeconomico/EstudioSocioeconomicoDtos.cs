namespace WebApplication2.Core.DTOs.EstudioSocioeconomico
{
    public class CatalogoItemDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CatalogosEstudioDto
    {
        public List<CatalogoItemDto> Parentescos { get; set; } = new();
        public List<CatalogoItemDto> ServiciosVivienda { get; set; } = new();
        public List<CatalogoItemDto> ServiciosMedicos { get; set; } = new();
        public List<CatalogoItemDto> RecursosTecnologicos { get; set; } = new();
    }

    public class AnalistaDto
    {
        public string Id { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class EstudioSocioeconomicoRequest
    {
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

        public List<int> ServiciosViviendaIds { get; set; } = new();
        public List<int> RecursosTecnologicosIds { get; set; } = new();
    }

    public class EstudioSocioeconomicoDto
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
        public string? AnalistaNombre { get; set; }
        public DateTime? FechaLlenado { get; set; }
        public bool LlenadoPorAspirante { get; set; }
        public string? Token { get; set; }

        public List<int> ServiciosViviendaIds { get; set; } = new();
        public List<int> RecursosTecnologicosIds { get; set; } = new();
    }

    public class EstudioPublicoDto
    {
        public string AspiranteNombre { get; set; } = string.Empty;
        public string? Carrera { get; set; }
        public string? Campus { get; set; }
        public bool YaEnviado { get; set; }
        public CatalogosEstudioDto Catalogos { get; set; } = new();
        public EstudioSocioeconomicoDto Estudio { get; set; } = new();
    }
}
