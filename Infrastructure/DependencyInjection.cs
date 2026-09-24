using Application.Imports;
using Application.Orders;
using Domain.Resources;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, string language)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.MissingConnectionString, language), nameof(connectionString));
        services.AddDbContext<SyncForgeDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
        services.AddScoped<IImportJobRepository, ImportJobRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }
}
