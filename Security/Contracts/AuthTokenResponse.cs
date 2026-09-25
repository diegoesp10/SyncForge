namespace Security.Contracts;

public sealed record AuthTokenResponse(
    string AccessToken, string TokenType, DateTimeOffset ExpiresAt,
    bool IsFirstLogin, bool ShouldShowOnboarding);
