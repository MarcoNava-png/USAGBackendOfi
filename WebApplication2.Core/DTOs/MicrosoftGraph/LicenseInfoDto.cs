namespace WebApplication2.Core.DTOs.MicrosoftGraph
{
    public class LicenseInfoDto
    {
        public string SkuId { get; set; } = string.Empty;
        public string SkuPartNumber { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalUnits { get; set; }
        public int ConsumedUnits { get; set; }
        public int AvailableUnits { get; set; }
    }
}
