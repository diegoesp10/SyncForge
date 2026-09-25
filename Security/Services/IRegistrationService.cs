using Security.Contracts;

namespace Security.Services;

public interface IRegistrationService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task ResendConfirmationAsync(ResendConfirmationRequest request, CancellationToken cancellationToken);
    Task ConfirmEmailAsync(Guid userId, string token);
}
