namespace WebApplication2.Core.Requests.PlanEstudios
{
    public class PlanEstudiosRequest
    {
        public string ClavePlanEstudios { get; set; }
        public string NombrePlanEstudios { get; set; }
        public string RVOE { get; set; }
        public bool PermiteAdelantar { get; set; }
        public string Version { get; set; }
        public int DuracionMeses { get; set; }
        public int MinimaAprobatoriaParcial { get; set; }
        public int MinimaAprobatoriaFinal { get; set; }
        public int IdPeriodicidad { get; set; }
        public int IdNivelEducativo { get; set; }
        public int IdCampus {  get; set; }
    }
}
