namespace WebApplication2.Core.Models
{
    public class AspiranteBitacoraSeguimiento : BaseEntity
    {
        public int Id { get; set; }
        public int AspiranteId { get; set; }
        public string UsuarioAtiendeId { get; set; }
        public DateTime Fecha { get; set; }
        public string MedioContacto { get; set; }
        public string Resumen { get; set; }
        public string ProximaAccion { get; set; }

        public virtual Aspirante Aspirante { get; set; }
        public virtual ApplicationUser UsuarioAtiende { get; set; }
    }
}
