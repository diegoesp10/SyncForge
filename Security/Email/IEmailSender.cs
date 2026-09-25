namespace Security.Email;

public interface IEmailSender
{
    bool IsConfigured { get; }
    Task SendConfirmationAsync(string email, Guid userId, string token, CancellationToken cancellationToken);
}
