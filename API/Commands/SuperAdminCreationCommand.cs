using Security.Identity;
using Security.Resources;
using Security.Services;

namespace API.Commands;

public static class SuperAdminCreationCommand
{
    private const string CommandName = "--SuperAdminCreation";

    public static async Task<bool> ExecuteIfRequestedAsync(WebApplication app, string[] args)
    {
        if (!args.Contains(CommandName, StringComparer.OrdinalIgnoreCase))
            return false;

        if (Console.IsInputRedirected)
            throw new InvalidOperationException(
                SecurityErrorMessages.Get(SecurityErrorCode.InteractiveTerminalRequired, "en"));

        Console.WriteLine($"Set the password for {InitialSuperAdmin.UserName} ({InitialSuperAdmin.Email}).");
        Console.Write("Password: ");
        var password = ReadPassword();
        Console.Write("Confirm password: ");
        var confirmation = ReadPassword();
        if (password != confirmation)
            throw new InvalidOperationException(SecurityErrorMessages.Get(SecurityErrorCode.PasswordsDoNotMatch, "en"));

        using var scope = app.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<SuperAdminCreationService>()
            .SetInitialPasswordAsync(password);
        Console.WriteLine("Password set. Confirm the email address before logging in.");
        return true;
    }

    private static string ReadPassword()
    {
        var characters = new List<char>();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                var password = new string(characters.ToArray());
                characters.Clear();
                return password;
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
}
