using AmCart.UserService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AmCart.UserService.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(string email, string firstName, string verificationLink, CancellationToken ct = default)
    {
        _logger.LogInformation("SendEmailVerification: To={Email}, Link={Link}", email, verificationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string firstName, string resetLink, CancellationToken ct = default)
    {
        _logger.LogInformation("SendPasswordReset: To={Email}, Link={Link}", email, resetLink);
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken ct = default)
    {
        _logger.LogInformation("SendWelcomeEmail: To={Email}", email);
        return Task.CompletedTask;
    }
}
