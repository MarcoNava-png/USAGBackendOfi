namespace WebApplication2.Core.DTOs
{
    public class GrupoMateriaDto
    {
        public int IdGrupoMateria { get; set; }

        public string Materia { get; set; }

        public string? Aula { get; set; }

        public short Cupo { get; set; }

        public string Profesor { get; set; }
    }
}
