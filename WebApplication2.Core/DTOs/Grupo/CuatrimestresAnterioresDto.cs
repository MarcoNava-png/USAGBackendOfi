namespace WebApplication2.Core.DTOs.Grupo
{
    public class CuatrimestresAnterioresPreviewDto
    {
        public int IdGrupoOrigen { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public string CodigoGrupo { get; set; } = string.Empty;
        public int IdPlanEstudios { get; set; }
        public string PlanEstudios { get; set; } = string.Empty;
        public int NumeroCuatrimestreActual { get; set; }
        public int NumeroGrupo { get; set; }
        public int IdTurno { get; set; }
        public string Turno { get; set; } = string.Empty;
        public int IdPeriodoActual { get; set; }
        public string PeriodoActual { get; set; } = string.Empty;
        public int TotalEstudiantes { get; set; }
        public List<CuatrimestrePrevioDto> CuatrimestresPrevios { get; set; } = new();
    }

    public class CuatrimestrePrevioDto
    {
        public int NumeroCuatrimestre { get; set; }
        public int TotalMateriasEnPlan { get; set; }
        public int? IdGrupoExistente { get; set; }
        public string? GrupoExistenteInfo { get; set; }
        public int? AlumnosCohorteInscritos { get; set; }
        public int? IdPeriodoSugerido { get; set; }
        public string? PeriodoSugerido { get; set; }
    }

    public class GenerarCuatrimestresAnterioresRequest
    {
        public int IdGrupoOrigen { get; set; }
        public bool CopiarEstudiantes { get; set; } = true;
        public List<CuatrimestreAGenerarDto> Cuatrimestres { get; set; } = new();
    }

    public class CuatrimestreAGenerarDto
    {
        public int NumeroCuatrimestre { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public NuevoPeriodoDto? NuevoPeriodo { get; set; }
    }

    public class NuevoPeriodoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Clave { get; set; }
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
    }

    public class GenerarCuatrimestresAnterioresResultado
    {
        public int IdGrupoOrigen { get; set; }
        public int TotalGruposCreados { get; set; }
        public int TotalGruposReutilizados { get; set; }
        public int TotalPeriodosCreados { get; set; }
        public List<CuatrimestreGeneradoDto> Cuatrimestres { get; set; } = new();
    }

    public class CuatrimestreGeneradoDto
    {
        public int NumeroCuatrimestre { get; set; }
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public string CodigoGrupo { get; set; } = string.Empty;
        public int IdPeriodoAcademico { get; set; }
        public string PeriodoAcademico { get; set; } = string.Empty;
        public bool GrupoYaExistia { get; set; }
        public bool PeriodoCreado { get; set; }
        public int TotalMaterias { get; set; }
        public int EstudiantesInscritos { get; set; }
        public int EstudiantesConAdvertencia { get; set; }
        public List<string> Advertencias { get; set; } = new();
    }
}
