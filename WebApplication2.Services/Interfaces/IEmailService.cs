namespace WebApplication2.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl);
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
