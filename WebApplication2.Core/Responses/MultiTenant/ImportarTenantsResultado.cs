using WebApplication2.Core.DTOs.MultiTenant;

namespace WebApplication2.Core.Responses.MultiTenant
{
    public class ImportarTenantsResultado
    {
        public int TotalFilas { get; set; }
        public int Exitosos { get; set; }
        public int Fallidos { get; set; }
        public List<ImportarTenantResultadoFila> Resultados { get; set; } = new();
    }
}
