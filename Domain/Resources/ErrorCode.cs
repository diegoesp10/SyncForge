namespace Domain.Resources;

public enum ErrorCode
{
    RequiredValue,
    ValueTooLong,
    InvalidFormat,
    InvalidStatus,
    InvalidTransition,
    ImportJobNotFound,
    InvalidPagination,
    MissingConnectionString,
    StatusPending,
    StatusProcessing,
    StatusCompleted,
    StatusFailed
}
