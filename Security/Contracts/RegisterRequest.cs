namespace Security.Contracts;

public sealed record RegisterRequest(string Email, string DisplayName, string Password);
