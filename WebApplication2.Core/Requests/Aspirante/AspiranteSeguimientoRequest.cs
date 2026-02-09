namespace WebApplication2.Core.Requests.Aspirante
{
    public class AspiranteSeguimientoRequest
    {
        public int AspiranteId { get; set; }
        public string UsuarioAtiendeId { get; set; }
        public DateTime Fecha { get; set; }
        public string MedioContacto { get; set; }
        public string Resumen { get; set; }
        public string ProximaAccion { get; set; }
    }
}
