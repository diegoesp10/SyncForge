using Application.Imports;
using Application.Orders;
using Application.Files;
using Application.Health;
using Domain.Resources;
using Infrastructure.Persistence;
using Infrastructure.Files;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, string language, string storageRoot)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.MissingConnectionString, language), nameof(connectionString));
        services.AddDbContext<SyncForgeDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<IImportJobRepository, ImportJobRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IHealthProbe, SqlHealthProbe>();
        services.AddSingleton<IFileStorage>(new LocalFileStorage(storageRoot));
        services.AddSingleton<IFileWorkQueue, FileWorkQueue>();
        services.AddHostedService<FileProcessingWorker>();
        return services;
    }
}
