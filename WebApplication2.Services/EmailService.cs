using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using WebApplication2.Core.Requests.MicrosoftGraph;
using WebApplication2.Services.Interfaces;

namespace WebApplication2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IMicrosoftGraphService? _graphService;
        private readonly bool _useSmtp;
        private readonly bool _useGraph;
        private readonly string? _fromEmail;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IMicrosoftGraphService? graphService = null)
        {
            _configuration = configuration;
            _logger = logger;
            _graphService = graphService;
            _useSmtp = !string.IsNullOrEmpty(_configuration["Email:SmtpHost"]);
            _fromEmail = _configuration["Email:FromEmail"];

            // Usar Graph si hay servicio disponible y un email remitente configurado
            _useGraph = _graphService != null && !string.IsNullOrEmpty(_fromEmail);

            if (_useGraph)
                _logger.LogInformation("EmailService: Usando Microsoft Graph API con remitente {FromEmail}", _fromEmail);
            else if (_useSmtp)
                _logger.LogInformation("EmailService: Usando SMTP con host {SmtpHost}", _configuration["Email:SmtpHost"]);
            else
                _logger.LogWarning("EmailService: Sin configuración de correo, operando en modo desarrollo (consola)");
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, string resetUrl)
        {
            var subject = "Restablecer Contraseña - Sistema Escolar USAG";

            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #2563eb, #1d4ed8); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f8fafc; padding: 30px; border: 1px solid #e2e8f0; }}
        .button {{ display: inline-block; background: #2563eb; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; margin: 20px 0; }}
        .button:hover {{ background: #1d4ed8; }}
        .footer {{ text-align: center; padding: 20px; color: #64748b; font-size: 12px; }}
        .code {{ background: #f1f5f9; padding: 15px; border-radius: 8px; font-family: monospace; font-size: 18px; text-align: center; letter-spacing: 2px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Restablecer Contraseña</h1>
        </div>
        <div class='content'>
            <p>Hola,</p>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
            <p>Haz clic en el siguiente botón para crear una nueva contraseña:</p>
            <p style='text-align: center;'>
                <a href='{resetUrl}' class='button'>Restablecer Contraseña</a>
            </p>
            <p>O copia y pega este enlace en tu navegador:</p>
            <p style='word-break: break-all; color: #2563eb;'>{resetUrl}</p>
            <p><strong>Este enlace expirará en 1 hora.</strong></p>
            <p>Si no solicitaste este cambio, puedes ignorar este correo. Tu contraseña permanecerá igual.</p>
        </div>
        <div class='footer'>
            <p>Sistema de Gestión Escolar USAG</p>
            <p>Este es un correo automático, por favor no responder.</p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (_useGraph)
            {
                await SendGraphEmailAsync(toEmail, subject, htmlBody);
            }
            else if (_useSmtp)
            {
                await SendSmtpEmailAsync(toEmail, subject, htmlBody);
            }
            else
            {
                _logger.LogWarning("========================================");
                _logger.LogWarning("EMAIL SERVICE - MODO DESARROLLO");
                _logger.LogWarning("========================================");
                _logger.LogWarning("Para: {ToEmail}", toEmail);
                _logger.LogWarning("Asunto: {Subject}", subject);
                _logger.LogWarning("----------------------------------------");
                _logger.LogWarning("Contenido del email enviado (ver HTML para link de reset):");

                var urlStart = htmlBody.IndexOf("href='") + 6;
                var urlEnd = htmlBody.IndexOf("'", urlStart);
                if (urlStart > 5 && urlEnd > urlStart)
                {
                    var resetUrl = htmlBody.Substring(urlStart, urlEnd - urlStart);
                    _logger.LogWarning("========================================");
                    _logger.LogWarning("LINK DE RESET: {ResetUrl}", resetUrl);
                    _logger.LogWarning("========================================");
                }

                _logger.LogInformation("Email simulado enviado exitosamente a {ToEmail}", toEmail);

                await Task.CompletedTask;
            }
        }

        private async Task SendGraphEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var request = new SendEmailRequest
            {
                To = toEmail,
                Subject = subject,
                Body = htmlBody,
                IsHtml = true
            };

            await _graphService!.SendEmailAsync(_fromEmail!, request);

            _logger.LogInformation("Email enviado vía Microsoft Graph desde {From} a {To}", _fromEmail, toEmail);
        }

        private async Task SendSmtpEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "Sistema Escolar USAG";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Email enviado vía SMTP a {ToEmail}", toEmail);
        }
    }
}
