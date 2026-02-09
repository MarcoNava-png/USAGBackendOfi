namespace WebApplication2.Core.DTOs
{
    public class AspiranteSeguimientoDto
    {
        public int Id { get; set; }
        public string UsuarioAtiendeId { get; set; }
        public string UsuarioAtiendeNombre {  get; set; }
        public DateTime Fecha { get; set; }
        public string MedioContacto { get; set; }
        public string Resumen { get; set; }
        public string ProximaAccion { get; set; }
    }
}
