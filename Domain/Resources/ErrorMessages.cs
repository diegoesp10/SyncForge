using System.Globalization;
using System.Resources;
using Domain.Imports.Enums;
using Domain.Files;

namespace Domain.Resources;

public static class ErrorMessages
{
    private static readonly ResourceManager Manager = new("Domain.Resources.Errors", typeof(ErrorMessages).Assembly);

    public static string Get(ErrorCode code, string? language, params object?[] values)
    {
        var culture = CultureFor(language);
        var template = Manager.GetString(code.ToString(), culture)
            ?? throw new MissingManifestResourceException(code.ToString());
        return string.Format(culture, template, values);
    }

    public static string Status(ImportStatus status, string? language) =>
        Get(status switch
        {
            ImportStatus.Pending => ErrorCode.StatusPending,
            ImportStatus.Processing => ErrorCode.StatusProcessing,
            ImportStatus.Completed => ErrorCode.StatusCompleted,
            ImportStatus.Failed => ErrorCode.StatusFailed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), Get(ErrorCode.InvalidStatus, language))
        }, language);

    public static string Failure(ImportFailureCode failureCode, string? language) =>
        Get(failureCode switch
        {
            ImportFailureCode.Unknown => ErrorCode.FailureUnknown,
            ImportFailureCode.InvalidFile => ErrorCode.FailureInvalidFile,
            ImportFailureCode.ValidationFailed => ErrorCode.FailureValidationFailed,
            ImportFailureCode.ProcessingFailed => ErrorCode.FailureProcessingFailed,
            _ => throw new ArgumentOutOfRangeException(nameof(failureCode), Get(ErrorCode.InvalidFailureCode, language))
        }, language);

    public static string FileFailure(FileFailureCode failureCode, string fileName, string? language) =>
        failureCode switch
        {
            FileFailureCode.UnsupportedFormat => Get(ErrorCode.UnsupportedFileFormat, language, Path.GetExtension(fileName)),
            FileFailureCode.InvalidContent => Get(ErrorCode.InvalidFileContent, language),
            FileFailureCode.MissingContent => Get(ErrorCode.MissingFileContent, language),
            FileFailureCode.ProcessingFailed => Get(ErrorCode.FileProcessingFailed, language),
            _ => throw new ArgumentOutOfRangeException(nameof(failureCode), Get(ErrorCode.InvalidFailureCode, language))
        };

    private static CultureInfo CultureFor(string? language) =>
        language?.StartsWith("es", StringComparison.OrdinalIgnoreCase) == true
            ? CultureInfo.GetCultureInfo("es")
            : CultureInfo.GetCultureInfo("en");
}
