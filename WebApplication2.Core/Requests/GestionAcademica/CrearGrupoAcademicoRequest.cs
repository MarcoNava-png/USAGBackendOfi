namespace WebApplication2.Core.Requests.GestionAcademica
{
    public class CrearGrupoAcademicoRequest
    {
        public int IdPlanEstudios { get; set; }
        public int IdPeriodoAcademico { get; set; }
        public int NumeroCuatrimestre { get; set; }
        public int NumeroGrupo { get; set; }
        public int IdTurno { get; set; }
        public int CapacidadMaxima { get; set; }
        public bool CargarMateriasAutomaticamente { get; set; } = true;
    }
}
