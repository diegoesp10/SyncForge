using Application.Imports.Commands;
using Application.Imports.Queries;
using Application.Imports;
using Application.Orders;
using Application.Orders.Commands;
using Application.Orders.Queries;
using Application.Files;
using Application.Health;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateImportJobHandler>();
        services.AddScoped<StartImportJobHandler>();
        services.AddScoped<CompleteImportJobHandler>();
        services.AddScoped<FailImportJobHandler>();
        services.AddScoped<RetryImportJobHandler>();
        services.AddScoped<GetImportJobHandler>();
        services.AddScoped<ListImportJobsHandler>();
        services.AddScoped<CreateOrderHandler>();
        services.AddScoped<GetOrderHandler>();
        services.AddScoped<GetOrderBySourceHandler>();
        services.AddScoped<ListOrdersHandler>();
        services.AddScoped<IImportJobService, ImportJobService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IFileAnalyzer, FileAnalyzer>();
        services.AddScoped<FileProcessingService>();
        services.AddScoped<IHealthService, HealthService>();
        return services;
    }
}
