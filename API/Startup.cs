using API.Errors;
using Application;
using Domain.Resources;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace API;

public sealed class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = configuration.GetConnectionString("SyncForge");

        services.AddApplication();
        services.AddInfrastructure(connectionString ?? string.Empty, "en");
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
