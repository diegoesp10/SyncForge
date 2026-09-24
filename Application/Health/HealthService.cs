using Contracts.Files;

namespace Application.Health;

public sealed class HealthService(IHealthProbe probe) : IHealthService
{
    public async Task<HealthResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var healthy = await probe.CanConnectAsync(cancellationToken);
        var version = typeof(HealthService).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";
        return new HealthResponse(healthy ? "ok" : "down", version);
    }
}
