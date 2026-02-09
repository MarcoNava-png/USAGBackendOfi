using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using WebApplication2.Core.DTOs.MicrosoftGraph;
using WebApplication2.Core.Requests.MicrosoftGraph;
using WebApplication2.Core.Responses.MicrosoftGraph;

namespace WebApplication2.Services;

public interface IMicrosoftGraphService
{
    Task<List<EmailDto>> GetEmailsAsync(string userEmail, int top = 50, bool unreadOnly = false, CancellationToken ct = default);
    Task<EmailDto?> GetEmailByIdAsync(string userEmail, string messageId, CancellationToken ct = default);
    Task SendEmailAsync(string fromUserEmail, SendEmailRequest request, CancellationToken ct = default);
    Task MarkAsReadAsync(string userEmail, string messageId, CancellationToken ct = default);
    Task<List<EmailDto>> SearchEmailsAsync(string userEmail, string searchQuery, int top = 50, CancellationToken ct = default);
    Task<List<UserInfoDto>> GetUsersAsync(int top = 100, CancellationToken ct = default);
    Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<bool> DeleteUserAsync(string userId, CancellationToken ct = default);
    Task<bool> UpdateUserAsync(string userId, CreateUserRequest request, CancellationToken ct = default);
    Task<UserInfoDto?> GetUserByIdAsync(string userIdOrEmail, CancellationToken ct = default);
    Task<string> ResetPasswordAsync(string userId, CancellationToken ct = default);
    Task<List<string>> GetDomainsAsync(CancellationToken ct = default);
}

public class MicrosoftGraphService : IMicrosoftGraphService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<MicrosoftGraphService> _logger;
    private readonly MicrosoftGraphSettings _settings;

    public MicrosoftGraphService(
        IConfiguration configuration,
        ILogger<MicrosoftGraphService> logger)
    {
        _logger = logger;

        _settings = new MicrosoftGraphSettings();
        configuration.GetSection("MicrosoftGraph").Bind(_settings);

        if (string.IsNullOrEmpty(_settings.TenantId) ||
            string.IsNullOrEmpty(_settings.ClientId) ||
            string.IsNullOrEmpty(_settings.ClientSecret))
        {
            throw new InvalidOperationException(
                "MicrosoftGraph settings are not configured. " +
                "Please configure TenantId, ClientId, and ClientSecret in appsettings.json");
        }

        var credential = new ClientSecretCredential(
            _settings.TenantId,
            _settings.ClientId,
            _settings.ClientSecret);

        _graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

        _logger.LogInformation("Microsoft Graph Service initialized for tenant {TenantId}", _settings.TenantId);
    }

    public async Task<List<EmailDto>> GetEmailsAsync(
        string userEmail,
        int top = 50,
        bool unreadOnly = false,
        CancellationToken ct = default)
    {
        try
        {
            var request = _graphClient.Users[userEmail].Messages;

            MessageCollectionResponse? messages;

            if (unreadOnly)
            {
                messages = await request.GetAsync(config =>
                {
                    config.QueryParameters.Top = top;
                    config.QueryParameters.Filter = "isRead eq false";
                    config.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
                    config.QueryParameters.Select = new[]
                    {
                        "id", "subject", "from", "toRecipients", "receivedDateTime",
                        "bodyPreview", "isRead", "hasAttachments", "importance"
                    };
                }, ct);
            }
            else
            {
                messages = await request.GetAsync(config =>
                {
                    config.QueryParameters.Top = top;
                    config.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
                    config.QueryParameters.Select = new[]
                    {
                        "id", "subject", "from", "toRecipients", "receivedDateTime",
                        "bodyPreview", "isRead", "hasAttachments", "importance"
                    };
                }, ct);
            }

            if (messages?.Value == null)
                return new List<EmailDto>();

            return messages.Value.Select(MapToEmailDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emails for user {UserEmail}", userEmail);
            throw;
        }
    }

    public async Task<EmailDto?> GetEmailByIdAsync(
        string userEmail,
        string messageId,
        CancellationToken ct = default)
    {
        try
        {
            var message = await _graphClient.Users[userEmail]
                .Messages[messageId]
                .GetAsync(config =>
                {
                    config.QueryParameters.Select = new[]
                    {
                        "id", "subject", "from", "toRecipients", "receivedDateTime",
                        "body", "bodyPreview", "isRead", "hasAttachments", "importance"
                    };
                }, ct);

            if (message == null)
                return null;

            var dto = MapToEmailDto(message);
            dto.BodyContent = message.Body?.Content;
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email {MessageId} for user {UserEmail}", messageId, userEmail);
            throw;
        }
    }

    public async Task SendEmailAsync(
        string fromUserEmail,
        SendEmailRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var message = new Message
            {
                Subject = request.Subject,
                Body = new ItemBody
                {
                    ContentType = request.IsHtml ? BodyType.Html : BodyType.Text,
                    Content = request.Body
                },
                ToRecipients = new List<Recipient>
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress { Address = request.To }
                    }
                }
            };

            if (request.Cc != null && request.Cc.Any())
            {
                message.CcRecipients = request.Cc.Select(cc => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = cc }
                }).ToList();
            }

            await _graphClient.Users[fromUserEmail]
                .SendMail
                .PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = true
                }, cancellationToken: ct);

            _logger.LogInformation("Email sent from {From} to {To}: {Subject}",
                fromUserEmail, request.To, request.Subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email from {FromUser} to {To}",
                fromUserEmail, request.To);
            throw;
        }
    }

    public async Task MarkAsReadAsync(
        string userEmail,
        string messageId,
        CancellationToken ct = default)
    {
        try
        {
            await _graphClient.Users[userEmail]
                .Messages[messageId]
                .PatchAsync(new Message { IsRead = true }, cancellationToken: ct);

            _logger.LogInformation("Marked email {MessageId} as read for {UserEmail}",
                messageId, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking email as read: {MessageId}", messageId);
            throw;
        }
    }

    public async Task<List<EmailDto>> SearchEmailsAsync(
        string userEmail,
        string searchQuery,
        int top = 50,
        CancellationToken ct = default)
    {
        try
        {
            var messages = await _graphClient.Users[userEmail]
                .Messages
                .GetAsync(config =>
                {
                    config.QueryParameters.Top = top;
                    config.QueryParameters.Search = $"\"{searchQuery}\"";
                    config.QueryParameters.Select = new[]
                    {
                        "id", "subject", "from", "toRecipients", "receivedDateTime",
                        "bodyPreview", "isRead", "hasAttachments", "importance"
                    };
                }, ct);

            if (messages?.Value == null)
                return new List<EmailDto>();

            return messages.Value.Select(MapToEmailDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching emails for user {UserEmail}", userEmail);
            throw;
        }
    }

    public async Task<List<UserInfoDto>> GetUsersAsync(int top = 100, CancellationToken ct = default)
    {
        try
        {
            var users = await _graphClient.Users
                .GetAsync(config =>
                {
                    config.QueryParameters.Top = top;
                    config.QueryParameters.Select = new[]
                    {
                        "id", "displayName", "mail", "userPrincipalName", "jobTitle", "department"
                    };
                }, ct);

            if (users?.Value == null)
                return new List<UserInfoDto>();

            return users.Value.Select(u => new UserInfoDto
            {
                Id = u.Id ?? "",
                DisplayName = u.DisplayName,
                Email = u.Mail ?? u.UserPrincipalName,
                JobTitle = u.JobTitle,
                Department = u.Department
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users from directory");
            throw;
        }
    }

    private static EmailDto MapToEmailDto(Message message)
    {
        return new EmailDto
        {
            Id = message.Id ?? "",
            Subject = message.Subject,
            From = message.From?.EmailAddress?.Name,
            FromEmail = message.From?.EmailAddress?.Address,
            ToRecipients = message.ToRecipients?
                .Select(r => r.EmailAddress?.Address ?? "")
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList() ?? new List<string>(),
            ReceivedDateTime = message.ReceivedDateTime?.DateTime,
            BodyPreview = message.BodyPreview,
            IsRead = message.IsRead ?? false,
            HasAttachments = message.HasAttachments ?? false,
            Importance = message.Importance?.ToString()
        };
    }

    public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        try
        {
            var user = new User
            {
                AccountEnabled = true,
                DisplayName = request.DisplayName,
                MailNickname = request.MailNickname,
                UserPrincipalName = request.UserPrincipalName,
                GivenName = request.GivenName,
                Surname = request.Surname,
                JobTitle = request.JobTitle,
                Department = request.Department,
                MobilePhone = request.MobilePhone,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = request.ForceChangePasswordNextSignIn,
                    Password = request.Password
                }
            };

            var createdUser = await _graphClient.Users.PostAsync(user, cancellationToken: ct);

            _logger.LogInformation("Usuario creado en Azure AD: {UserPrincipalName}", request.UserPrincipalName);

            return new CreateUserResponse
            {
                Success = true,
                UserId = createdUser?.Id,
                UserPrincipalName = createdUser?.UserPrincipalName,
                Email = createdUser?.Mail ?? createdUser?.UserPrincipalName,
                Message = "Usuario creado exitosamente"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario {UserPrincipalName}", request.UserPrincipalName);
            return new CreateUserResponse
            {
                Success = false,
                Message = $"Error al crear usuario: {ex.Message}"
            };
        }
    }

    public async Task<bool> DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            await _graphClient.Users[userId].DeleteAsync(cancellationToken: ct);
            _logger.LogInformation("Usuario eliminado de Azure AD: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateUserAsync(string userId, CreateUserRequest request, CancellationToken ct = default)
    {
        try
        {
            var user = new User
            {
                DisplayName = request.DisplayName,
                GivenName = request.GivenName,
                Surname = request.Surname,
                JobTitle = request.JobTitle,
                Department = request.Department,
                MobilePhone = request.MobilePhone
            };

            await _graphClient.Users[userId].PatchAsync(user, cancellationToken: ct);
            _logger.LogInformation("Usuario actualizado en Azure AD: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {UserId}", userId);
            return false;
        }
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(string userIdOrEmail, CancellationToken ct = default)
    {
        try
        {
            var user = await _graphClient.Users[userIdOrEmail]
                .GetAsync(config =>
                {
                    config.QueryParameters.Select = new[]
                    {
                        "id", "displayName", "mail", "userPrincipalName",
                        "givenName", "surname", "jobTitle", "department", "mobilePhone"
                    };
                }, ct);

            if (user == null) return null;

            return new UserInfoDto
            {
                Id = user.Id ?? "",
                DisplayName = user.DisplayName,
                Email = user.Mail ?? user.UserPrincipalName,
                JobTitle = user.JobTitle,
                Department = user.Department
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {UserIdOrEmail}", userIdOrEmail);
            return null;
        }
    }

    public async Task<string> ResetPasswordAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var newPassword = GenerateRandomPassword();

            var user = new User
            {
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = true,
                    Password = newPassword
                }
            };

            await _graphClient.Users[userId].PatchAsync(user, cancellationToken: ct);
            _logger.LogInformation("Contraseña reseteada para usuario {UserId}", userId);

            return newPassword;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al resetear contraseña de usuario {UserId}", userId);
            throw;
        }
    }

    public async Task<List<string>> GetDomainsAsync(CancellationToken ct = default)
    {
        try
        {
            var domains = await _graphClient.Domains.GetAsync(cancellationToken: ct);

            if (domains?.Value == null)
                return new List<string>();

            return domains.Value
                .Where(d => d.IsVerified == true)
                .Select(d => d.Id ?? "")
                .Where(d => !string.IsNullOrEmpty(d))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener dominios");
            return new List<string>();
        }
    }

    private static string GenerateRandomPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghjkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%&*";

        var random = new Random();
        var password = new char[14];

        password[0] = upper[random.Next(upper.Length)];
        password[1] = lower[random.Next(lower.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        var all = upper + lower + digits + special;
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = all[random.Next(all.Length)];
        }

        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }
}
