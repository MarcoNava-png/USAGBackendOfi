namespace WebApplication2.Core.Requests.Aspirante
{
    public class GenerarRecibosPlantillaAspiranteRequest
    {
        public int IdPlantillaCobro { get; set; }
        public bool EliminarPendientesExistentes { get; set; }
    }
}
