using Application.Imports.Commands;
using Application.Imports.Queries;
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
        return services;
    }
}
