namespace Application.Health;

public interface IHealthProbe
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
