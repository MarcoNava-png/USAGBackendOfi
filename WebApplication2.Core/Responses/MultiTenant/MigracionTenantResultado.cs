namespace WebApplication2.Core.Responses.MultiTenant
{
    public class MigracionTenantResultado
    {
        public int IdTenant { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public bool Exito { get; set; }
        public int MigracionesAplicadas { get; set; }
        public string? Error { get; set; }
    }

    public class MigrarTodosResultado
    {
        public int TotalTenants { get; set; }
        public int Exitosos { get; set; }
        public int ConError { get; set; }
        public int TotalMigracionesAplicadas { get; set; }
        public List<MigracionTenantResultado> Detalle { get; set; } = new();
    }
}
