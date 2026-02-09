namespace WebApplication2.Core.Requests.GestionAcademica
{
    public class PromoverEstudiantesRequest
    {
        public int IdGrupoActual { get; set; }
        public int IdPeriodoAcademicoDestino { get; set; }
        public bool? CrearGrupoSiguienteAutomaticamente { get; set; } = true;
        public decimal? PromedioMinimoPromocion { get; set; } = 70;
        public bool? PromoverTodos { get; set; } = false;
        public bool? ValidarPagos { get; set; } = true;
        public List<int>? EstudiantesExcluidos { get; set; }
    }
}
