using Contracts.Files;

namespace Application.Health;

public interface IHealthService
{
    Task<HealthResponse> GetAsync(CancellationToken cancellationToken = default);
}
