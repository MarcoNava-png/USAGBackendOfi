namespace WebApplication2.Core.Requests.MultiTenant
{
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
