namespace WebApplication2.Core.Models
{
    public class SolicitudProrrogaCaptura : BaseEntity
    {
        public int IdSolicitudProrroga { get; set; }
        public int IdProfesor { get; set; }
        public int IdGrupoMateria { get; set; }
        public int NumeroParcial { get; set; }
        public string? Motivo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public DateTime? FechaLimiteProrroga { get; set; }
        public string? ResueltaPor { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string? NotaResolucion { get; set; }

        public virtual Profesor? IdProfesorNavigation { get; set; }
        public virtual GrupoMateria? IdGrupoMateriaNavigation { get; set; }
    }
}
