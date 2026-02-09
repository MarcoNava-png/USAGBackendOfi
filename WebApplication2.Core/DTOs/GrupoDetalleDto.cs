namespace WebApplication2.Core.DTOs
{
    public class GrupoDetalleDto
    {
        public int IdGrupo { get; set; }
        public string Name { get; set; }

        public string PlanEstudios { get; set; }

        public string PeriodoAcademico { get; set; }

        public byte NumeroCuatrimestre { get; set; }

        public byte NumeroGrupo { get; set; }

        public string Turno { get; set; }

        public short CapacidadMaxima { get; set; }

        public string CodigoGrupo { get; set; }

        public virtual ICollection<GrupoMateriaDto> GrupoMateria { get; set; } = new List<GrupoMateriaDto>();
    }
}
