namespace WebApplication2.Core.Requests.MicrosoftGraph
{
    public class CreateUserRequest
    {
        public string DisplayName { get; set; } = null!;
        public string UserPrincipalName { get; set; } = null!;
        public string MailNickname { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool ForceChangePasswordNextSignIn { get; set; } = true;
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? MobilePhone { get; set; }
    }
}
