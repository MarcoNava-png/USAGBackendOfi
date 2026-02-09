namespace WebApplication2.Core.DTOs.Dashboard
{
    public class GrupoResumenDto
    {
        public int IdGrupo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Programa { get; set; } = string.Empty;
        public int Cuatrimestre { get; set; }
        public int TotalEstudiantes { get; set; }
        public decimal PromedioGeneral { get; set; }
    }
}
