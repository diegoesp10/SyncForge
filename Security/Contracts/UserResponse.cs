namespace Security.Contracts;

public sealed record UserResponse(
    Guid Id, string Email, string DisplayName, bool IsActive,
    IReadOnlyList<string> Roles, DateTimeOffset? LastSignedInAt);
