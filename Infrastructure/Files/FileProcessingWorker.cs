using Application.Files;
using Domain.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Files;

public sealed class FileProcessingWorker(
    IServiceScopeFactory scopeFactory,
    IFileWorkQueue queue,
    ILogger<FileProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<FileProcessingService>();
                var pending = await processor.RecoverQueuedAsync(stoppingToken);
                foreach (var id in pending)
                    await queue.EnqueueAsync(id, stoppingToken);
                break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "{Error}", ErrorMessages.Get(ErrorCode.FileProcessingFailed, "en"));
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        await foreach (var id in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<FileProcessingService>()
                    .ProcessAsync(id, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "{Error}: {FileId}",
                    ErrorMessages.Get(ErrorCode.FileProcessingFailed, "en"), id);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                await queue.EnqueueAsync(id, stoppingToken);
            }
        }
    }
}
