using System.Globalization;
using System.Resources;

namespace Security.Resources;

public static class SecurityErrorMessages
{
    private static readonly ResourceManager Manager = new("Security.Resources.SecurityErrors", typeof(SecurityErrorMessages).Assembly);

    public static string Get(SecurityErrorCode code, string? language)
    {
        var culture = language?.StartsWith("es", StringComparison.OrdinalIgnoreCase) == true
            ? CultureInfo.GetCultureInfo("es")
            : CultureInfo.GetCultureInfo("en");
        return Manager.GetString(code.ToString(), culture)
            ?? throw new MissingManifestResourceException(code.ToString());
    }
}
