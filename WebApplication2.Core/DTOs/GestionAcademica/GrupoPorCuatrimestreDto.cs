namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class GrupoPorCuatrimestreDto
    {
        public int NumeroCuatrimestre { get; set; }
        public List<GrupoResumenDto> Grupos { get; set; } = new();
    }
}
