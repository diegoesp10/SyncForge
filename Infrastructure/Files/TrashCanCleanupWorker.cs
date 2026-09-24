using Application.Files;
using Domain.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Files;

public sealed class TrashCanCleanupWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TrashCanCleanupWorker> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                IReadOnlyList<Guid> expired;
                using (var scope = scopeFactory.CreateScope())
                    expired = await scope.ServiceProvider.GetRequiredService<ITrashCanService>()
                        .ListExpiredIdsAsync(stoppingToken);

                foreach (var id in expired)
                {
                    try
                    {
                        using var scope = scopeFactory.CreateScope();
                        await scope.ServiceProvider.GetRequiredService<ITrashCanService>()
                            .PurgeAsync(id, "en", stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception, "{Error}: {FileId}",
                            ErrorMessages.Get(ErrorCode.TrashPurgeFailed, "en"), id);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "{Error}", ErrorMessages.Get(ErrorCode.TrashPurgeFailed, "en"));
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
