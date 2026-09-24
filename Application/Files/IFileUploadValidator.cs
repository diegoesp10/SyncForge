namespace Application.Files;

public interface IFileUploadValidator
{
    void ValidateMetadata(string fileName, string? contentType, string language);
    Task ValidateContentAsync(Stream content, string language, CancellationToken cancellationToken = default);
}
