namespace WebApplication2.Core.DTOs.Grupo
{
    public class DiasClaseMateriaDto
    {
        public int IdGrupoMateria { get; set; }
        public string NombreMateria { get; set; } = string.Empty;
        public List<string> DiasSemana { get; set; } = new();
        public List<HorarioClaseDto> Horarios { get; set; } = new();
    }
}
