using API.Errors;
using Application;
using Application.Files;
using Domain.Resources;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Security;

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
        services.AddSecurity(configuration);
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
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            };
            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
            foreach (var publicPath in new[]
                     { "/api/auth/login", "/api/auth/register", "/api/auth/resend-confirmation",
                       "/api/auth/confirm-email", "/api/health" })
                if (document.Paths.TryGetValue(publicPath, out var path) && path?.Operations is { } operations)
                    foreach (var operation in operations.Values)
                        operation.Security = [];
            return Task.CompletedTask;
        }));
        services.Configure<FormOptions>(options =>
            options.MultipartBodyLengthLimit = FileService.MaxFileSizeBytes + 1024 * 1024);
        services.Configure<KestrelServerOptions>(options =>
            options.Limits.MaxRequestBodySize = FileService.MaxFileSizeBytes + 1024 * 1024);
    }

    public void Configure(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
            app.MapScalarApiReference("/api-docs", options => options.WithTitle("SyncForge API")).AllowAnonymous();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
