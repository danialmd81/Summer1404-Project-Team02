namespace ETL.Application.Common.Constants;

public static class Policy
{
    public const string SystemAdminOnly = "SystemAdminOnly";
    public const string DataAdminOnly = "DataAdminOnly";
    public const string AnalystOnly = "AnalystOnly";
    public const string CanCreateUser = "CanCreateUser";
    public const string AuthenticatedUser = "AuthenticatedUser";
}