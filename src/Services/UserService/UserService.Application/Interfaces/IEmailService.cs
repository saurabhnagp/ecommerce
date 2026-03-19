namespace AmCart.UserService.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string firstName, string verificationLink, CancellationToken ct = default);
    Task SendPasswordResetAsync(string email, string firstName, string resetLink, CancellationToken ct = default);
    Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken ct = default);
}
