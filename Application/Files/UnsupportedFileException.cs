namespace Application.Files;

public sealed class UnsupportedFileException(string message) : Exception(message);
