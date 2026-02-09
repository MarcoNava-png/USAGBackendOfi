namespace WebApplication2.Core.Requests.MultiTenant
{
    public class LoginSuperAdminRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
