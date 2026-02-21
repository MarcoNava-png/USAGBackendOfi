namespace WebApplication2.Core.Responses.MicrosoftGraph
{
    public class CreateUserResponse
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? Email { get; set; }
        public string? Message { get; set; }
        public bool AppUserCreated { get; set; }
        public string? AppUserId { get; set; }
        public List<string>? AssignedRoles { get; set; }
        public bool CredentialsEmailSent { get; set; }
    }
}
