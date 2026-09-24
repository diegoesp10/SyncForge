using API.Errors;
using Application;
using Application.Files;
using Domain.Resources;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace API;

public sealed class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = configuration.GetConnectionString("SyncForge");

        services.AddApplication();
        var storagePath = configuration["FileStorage:Path"] ?? "data/uploads";
        var storageRoot = Path.IsPathRooted(storagePath)
            ? storagePath
            : Path.Combine(environment.ContentRootPath, storagePath);
        services.AddInfrastructure(connectionString ?? string.Empty, "en", storageRoot);
        services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var language = ApiLanguage.Resolve(context.HttpContext);
                return new BadRequestObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = ErrorMessages.Get(ErrorCode.BadRequestTitle, language),
                    Detail = ErrorMessages.Get(ErrorCode.InvalidRequest, language)
                });
            };
        });
        services.AddOpenApi();
        services.Configure<FormOptions>(options =>
            options.MultipartBodyLengthLimit = FileService.MaxFileSizeBytes + 1024 * 1024);
        services.Configure<KestrelServerOptions>(options =>
            options.Limits.MaxRequestBodySize = FileService.MaxFileSizeBytes + 1024 * 1024);
    }

    public void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            app.MapOpenApi();
        else
            app.UseHttpsRedirection();

        app.MapControllers();
    }
}
