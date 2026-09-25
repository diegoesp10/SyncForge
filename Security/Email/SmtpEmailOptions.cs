namespace Security.Email;

public sealed class SmtpEmailOptions
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public string? Sender { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ConfirmationBaseUrl { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host)
        && Port is > 0 and <= 65535
        && !string.IsNullOrWhiteSpace(Sender)
        && !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password)
        && Uri.TryCreate(ConfirmationBaseUrl, UriKind.Absolute, out var url)
        && url.Scheme == Uri.UriSchemeHttps;
}
