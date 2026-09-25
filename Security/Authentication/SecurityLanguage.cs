using Microsoft.AspNetCore.Http;

namespace Security.Authentication;

public static class SecurityLanguage
{
    public static string Resolve(HttpContext context)
    {
        var value = context.Request.Query["language"].ToString();
        if (string.IsNullOrWhiteSpace(value))
            value = context.Request.Headers.AcceptLanguage.ToString().Split(',')[0];
        return value.Trim().StartsWith("es", StringComparison.OrdinalIgnoreCase) ? "es" : "en";
    }
}
