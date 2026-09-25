namespace Security.Contracts;

public sealed record CreateUserRequest(string Email, string DisplayName, string Password, string Role);
