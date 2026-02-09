namespace WebApplication2.Core.DTOs.MultiTenant
{
    public class ConfiguracionAlertasDto
    {
        public int DiasAnticipacionPrimeraAlerta { get; set; } = 30;
        public int DiasAnticipacionSegundaAlerta { get; set; } = 15;
        public int DiasAnticipacionAlertaCritica { get; set; } = 7;
        public bool EnviarEmailAlerta { get; set; } = true;
        public bool EnviarEmailRecordatorio { get; set; } = true;
        public string? EmailDestinatarioAdicional { get; set; }
    }
}
