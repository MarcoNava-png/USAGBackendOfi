namespace WebApplication2.Core.DTOs.EstudiantePanel
{
    public class SeguimientoAcademicoDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string PlanEstudios { get; set; } = string.Empty;
        public List<PeriodoAcademicoDetalleDto> Periodos { get; set; } = new();
        public ResumenKardexDto ResumenGeneral { get; set; } = new();
    }
}
