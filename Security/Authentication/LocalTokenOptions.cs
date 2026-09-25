namespace Security.Authentication;

public sealed class LocalTokenOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required byte[] SigningKey { get; init; }
    public required TimeSpan Lifetime { get; init; }
}
