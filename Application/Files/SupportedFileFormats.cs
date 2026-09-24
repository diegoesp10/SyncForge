namespace Application.Files;

public static class SupportedFileFormats
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".csv", ".tsv", ".json", ".txt", ".log", ".xml", ".md"
    };

    public static bool Contains(string fileName) => Extensions.Contains(Path.GetExtension(fileName));
}
