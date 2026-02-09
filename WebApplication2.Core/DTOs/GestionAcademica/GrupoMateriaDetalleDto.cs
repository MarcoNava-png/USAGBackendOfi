namespace WebApplication2.Core.DTOs.GestionAcademica
{
    public class GrupoMateriaDetalleDto
    {
        public int IdGrupoMateria { get; set; }
        public string NombreGrupoMateria { get; set; } = string.Empty;
        public int IdMateriaPlan { get; set; }
        public string NombreMateria { get; set; } = string.Empty;
        public string ClaveMateria { get; set; } = string.Empty;
        public int Creditos { get; set; }
        public int? IdProfesor { get; set; }
        public string? NombreProfesor { get; set; }
        public string? Aula { get; set; }
        public short Cupo { get; set; }
        public int EstudiantesInscritos { get; set; }
        public int CupoDisponible { get; set; }
        public bool TieneCupo { get; set; }
        public List<HorarioItemDto>? HorarioJson { get; set; }
    }
}
