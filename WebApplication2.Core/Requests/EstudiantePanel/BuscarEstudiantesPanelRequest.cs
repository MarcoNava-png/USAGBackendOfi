namespace WebApplication2.Core.Requests.EstudiantePanel
{
    public class BuscarEstudiantesPanelRequest
    {
        public string? Busqueda { get; set; }
        public int? IdPlanEstudios { get; set; }
        public int? IdGrupo { get; set; }
        public int? IdPeriodoAcademico { get; set; }
        public bool? SoloActivos { get; set; } = true;
        public bool? ConAdeudo { get; set; }
        public bool? ConBeca { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 20;
    }
}
