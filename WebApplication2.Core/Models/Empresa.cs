namespace WebApplication2.Core.Models
{
    public class Empresa : BaseEntity
    {
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; } = null!;
        public bool Activo { get; set; } = true;

        public virtual ICollection<Aspirante> Aspirantes { get; set; } = new List<Aspirante>();
    }
}
