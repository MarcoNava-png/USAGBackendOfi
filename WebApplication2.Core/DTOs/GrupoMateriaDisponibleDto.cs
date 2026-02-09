namespace WebApplication2.Core.DTOs
{
    public class GrupoMateriaDisponibleDto
    {
        public int IdGrupoMateria { get; set; }
        public int IdGrupo { get; set; }
        public int IdMateriaPlan { get; set; }
        public string NombreMateria { get; set; } = null!;
        public string ClaveMateria { get; set; } = null!;
        public string Grupo { get; set; } = null!;
        public string? NombreProfesor { get; set; }
        public int CupoMaximo { get; set; }
        public int Inscritos { get; set; }
        public int Disponibles { get; set; }
        public string PeriodoAcademico { get; set; } = null!;
        public string? Horario { get; set; }
    }
}
