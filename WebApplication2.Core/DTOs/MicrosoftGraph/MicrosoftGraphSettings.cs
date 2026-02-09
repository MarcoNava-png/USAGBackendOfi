namespace WebApplication2.Core.DTOs.MicrosoftGraph
{
    public class MicrosoftGraphSettings
    {
        public string TenantId { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string? DefaultUserEmail { get; set; }
    }
}
