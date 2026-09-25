namespace Security.Identity;

public static class AppRoles
{
    public const string User = "User";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";

    public static readonly string[] All = [User, Admin, SuperAdmin];
}
