namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class GestionGruposPlanDto
    {
        public int IdPlanEstudios { get; set; }
        public string NombrePlan { get; set; } = string.Empty;
        public string ClavePlan { get; set; } = string.Empty;
        public int DuracionCuatrimestres { get; set; }
        public string Periodicidad { get; set; } = string.Empty;
        public List<GrupoPorCuatrimestreDto> GruposPorCuatrimestre { get; set; } = new();
    }
}
