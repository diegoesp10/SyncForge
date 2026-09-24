using Domain.Files;

namespace Application.Files;

public sealed class FileAnalysisException(FileFailureCode code) : Exception
{
    public FileFailureCode Code { get; } = code;
}
