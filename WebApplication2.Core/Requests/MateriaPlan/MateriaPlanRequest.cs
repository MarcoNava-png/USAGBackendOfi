namespace WebApplication2.Core.Requests.NewFolder
{
    public class MateriaPlanRequest
    {
        public int IdPlanEstudios { get; set; }
        public int IdMateria { get; set; }
        public int Cuatrimestre { get; set; }
        public bool EsOptativa { get; set; }
    }
}
