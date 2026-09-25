using API.Commands;

namespace API;

public static class SyncForgeHost
{
    public static async Task RunAsync(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        if (builder.Environment.IsDevelopment())
            builder.Configuration.AddJsonFile(
                Path.Combine(builder.Environment.ContentRootPath, "appsettings.Development.local.json"),
                optional: true, reloadOnChange: false);

        var startup = new Startup(builder.Configuration, builder.Environment);
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();
        if (await SuperAdminCreationCommand.ExecuteIfRequestedAsync(app, args))
            return;

        startup.Configure(app);
        await app.RunAsync();
    }
}
