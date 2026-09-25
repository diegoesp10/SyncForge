using API;
using Security.Resources;
using Security.Services;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile(
        Path.Combine(builder.Environment.ContentRootPath, "appsettings.Development.local.json"),
        optional: true, reloadOnChange: false);
var startup = new Startup(builder.Configuration, builder.Environment);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
if (args.Contains("--bootstrap-superadmin", StringComparer.OrdinalIgnoreCase))
{
    if (Console.IsInputRedirected)
        throw new InvalidOperationException(SecurityErrorMessages.Get(SecurityErrorCode.InteractiveTerminalRequired, "en"));

    Console.Write("Email: ");
    var email = Console.ReadLine() ?? string.Empty;
    Console.Write("Display name: ");
    var displayName = Console.ReadLine() ?? string.Empty;
    Console.Write("Password: ");
    var password = ReadPassword();
    Console.Write("Confirm password: ");
    var confirmation = ReadPassword();
    if (password != confirmation)
        throw new InvalidOperationException(SecurityErrorMessages.Get(SecurityErrorCode.PasswordsDoNotMatch, "en"));

    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<SecurityBootstrapper>()
        .CreateInitialSuperAdminAsync(email, displayName, password);
    Console.WriteLine("Initial SuperAdmin created.");
    return;
}
startup.Configure(app);
app.Run();

static string ReadPassword()
{
    var characters = new List<char>();
    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return new string(characters.ToArray());
        }
        if (key.Key == ConsoleKey.Backspace)
        {
            if (characters.Count > 0)
                characters.RemoveAt(characters.Count - 1);
            continue;
        }
        if (!char.IsControl(key.KeyChar))
            characters.Add(key.KeyChar);
    }
}
