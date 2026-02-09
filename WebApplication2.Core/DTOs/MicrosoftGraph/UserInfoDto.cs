namespace WebApplication2.Core.DTOs.MicrosoftGraph
{
    public class UserInfoDto
    {
        public string Id { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
    }
}
