namespace Security.Resources;

public sealed class SecurityOperationException(SecurityErrorCode code, int statusCode) : Exception(code.ToString())
{
    public SecurityErrorCode Code { get; } = code;
    public int StatusCode { get; } = statusCode;
}
