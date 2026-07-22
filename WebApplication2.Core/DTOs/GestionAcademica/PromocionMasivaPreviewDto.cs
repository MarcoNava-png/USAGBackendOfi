namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class PromocionMasivaPreviewDto
    {
        public int IdPeriodoOrigen { get; set; }
        public string PeriodoOrigen { get; set; } = string.Empty;
        public int IdPeriodoDestino { get; set; }
        public string PeriodoDestino { get; set; } = string.Empty;

        public int TotalGrupos { get; set; }
        public int TotalEstudiantes { get; set; }
        public int TotalAPromover { get; set; }
        public int TotalAEgresar { get; set; }
        public int TotalExcluidosNuevoIngreso { get; set; }
        public int TotalConAdeudo { get; set; }
        public decimal TotalSaldoPendiente { get; set; }
        public int TotalConError { get; set; }

        public List<PromocionMasivaGrupoDto> Grupos { get; set; } = new();
    }

    public class PromocionMasivaGrupoDto
    {
        public int IdGrupo { get; set; }
        public string Campus { get; set; } = string.Empty;
        public string PlanEstudios { get; set; } = string.Empty;
        public string CodigoGrupo { get; set; } = string.Empty;
        public string NombreGrupo { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
        public int CuatrimestreOrigen { get; set; }
        public int? CuatrimestreDestino { get; set; }
        public bool EsUltimoCuatrimestre { get; set; }

        public int TotalEstudiantes { get; set; }
        public int APromover { get; set; }
        public int AEgresar { get; set; }
        public int ExcluidosNuevoIngreso { get; set; }
        public int ConAdeudo { get; set; }
        public decimal SaldoPendiente { get; set; }
        public int ConError { get; set; }
    }
}
