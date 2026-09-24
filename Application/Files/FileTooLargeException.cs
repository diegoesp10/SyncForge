namespace Application.Files;

public sealed class FileTooLargeException(string message) : Exception(message);
