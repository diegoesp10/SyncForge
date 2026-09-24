namespace API.Errors;

internal static class ApiLanguage
{
    public static string Resolve(HttpContext context, string? requestedLanguage = null)
    {
        var value = requestedLanguage;
        if (string.IsNullOrWhiteSpace(value))
            value = context.Request.Query["language"].ToString();
        if (string.IsNullOrWhiteSpace(value))
            value = context.Request.Headers.AcceptLanguage.ToString().Split(',')[0];

        return value?.Trim().StartsWith("es", StringComparison.OrdinalIgnoreCase) == true ? "es" : "en";
    }
}
