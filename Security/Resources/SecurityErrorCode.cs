namespace Security.Resources;

public enum SecurityErrorCode
{
    InvalidCredentials,
    InvalidUserRequest,
    PasswordPolicyViolation,
    UserAlreadyExists,
    UserNotFound,
    InvalidRole,
    SuperAdminAlreadyExists,
    AuthenticationRequired,
    AccessDenied,
    SecurityDatabaseNotReady,
    SecurityConfigurationMissing,
    TooManyLoginAttempts,
    TooManyRegistrationAttempts,
    EmailDeliveryUnavailable,
    InvalidConfirmationToken,
    InteractiveTerminalRequired,
    PasswordsDoNotMatch
}
