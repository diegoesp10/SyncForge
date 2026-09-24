namespace Domain.Resources;

internal static class DomainValidation
{
    public static string Required(string? value, string name, int maxLength, string language)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.RequiredValue, language, name), name);

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new ArgumentException(ErrorMessages.Get(ErrorCode.ValueTooLong, language, name, maxLength), name);

        return trimmed;
    }
}
