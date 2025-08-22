namespace ETL.API.Infrastructure;

public static class Policies
{
    public const string SystemAdminOnly = "SystemAdminOnly";
    public const string DataAdminOnly = "DataAdminOnly";
    public const string AnalystOnly = "AnalystOnly";
    public const string CanManageUsers = "CanManageUsers";
    public const string AuthenticatedUser = "AuthenticatedUser";
}