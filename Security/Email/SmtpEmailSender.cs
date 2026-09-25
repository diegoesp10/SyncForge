using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Security.Resources;

namespace Security.Email;

public sealed class SmtpEmailSender(IOptions<SmtpEmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpEmailOptions settings = options.Value;

    public bool IsConfigured => settings.IsConfigured;

    public async Task SendConfirmationAsync(string email, Guid userId, string token, CancellationToken cancellationToken)
    {
        if (!IsConfigured)
            throw new SecurityOperationException(SecurityErrorCode.EmailDeliveryUnavailable, 503);

        var separator = settings.ConfirmationBaseUrl!.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        var link = $"{settings.ConfirmationBaseUrl}{separator}userId={userId:D}&token={Uri.EscapeDataString(token)}";
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(settings.Sender!));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = "Confirma tu correo de SyncForge / Confirm your SyncForge email";
        message.Body = new TextPart("plain")
        {
            Text = $"Confirma tu correo para activar tu cuenta de SyncForge:\n{link}\n\n"
                + $"Confirm your email to activate your SyncForge account:\n{link}\n\n"
                + "Si no solicitaste la cuenta, ignora este mensaje. / If you did not request the account, ignore this message."
        };

        try
        {
            using var client = new SmtpClient();
            var security = settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            await client.ConnectAsync(settings.Host!, settings.Port, security, cancellationToken);
            await client.AuthenticateAsync(settings.Username!, settings.Password!, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Could not send a SyncForge email confirmation message.");
            throw new SecurityOperationException(SecurityErrorCode.EmailDeliveryUnavailable, 503);
        }
    }
}
