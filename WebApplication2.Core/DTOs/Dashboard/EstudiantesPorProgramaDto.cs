namespace WebApplication2.Core.DTOs.Dashboard
{
    public class EstudiantesPorProgramaDto
    {
        public int IdPlanEstudios { get; set; }
        public string NombrePrograma { get; set; } = string.Empty;
        public int TotalEstudiantes { get; set; }
        public Dictionary<int, int> PorCuatrimestre { get; set; } = new();
    }
}
