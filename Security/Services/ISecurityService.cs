using Security.Contracts;

namespace Security.Services;

public interface ISecurityService
{
    Task<AuthTokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<UserResponse> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponse>> ListUsersAsync(CancellationToken cancellationToken = default);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> ChangeRoleAsync(Guid userId, ChangeUserRoleRequest request, CancellationToken cancellationToken = default);
    Task<UserResponse> SetStatusAsync(Guid userId, SetUserStatusRequest request, CancellationToken cancellationToken = default);
}
