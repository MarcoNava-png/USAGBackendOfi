namespace WebApplication2.Core.DTOs
{
    public class MateriaPlanDto
    {
        public int IdMateriaPlan { get; set; }
        public int IdPlanEstudios { get; set; }
        public string NombrePlanEstudios { get; set; }
        public int IdMateria { get; set; }
        public string Materia { get; set; }
        public string? ClaveMateria { get; set; }
        public string? NombreMateria { get; set; }
        public decimal Creditos { get; set; }
        public int Cuatrimestre { get; set; }
        public bool EsOptativa { get; set; }
    }
}
