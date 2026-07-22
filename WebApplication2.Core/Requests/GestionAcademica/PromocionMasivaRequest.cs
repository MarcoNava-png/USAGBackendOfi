namespace WebApplication2.Core.Requests.GestionAcademica
{
    public class PromocionMasivaRequest
    {
        public int IdPeriodoOrigen { get; set; }
        public int IdPeriodoDestino { get; set; }
        public List<int>? GruposExcluidos { get; set; }
        public List<int>? EstudiantesExcluidos { get; set; }
    }
}
