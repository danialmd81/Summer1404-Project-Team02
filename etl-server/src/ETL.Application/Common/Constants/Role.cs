namespace ETL.Application.Common.Constants;

public static class Role
{
    public const string SystemAdmin = "system-admin";
    public const string DataAdmin = "data-admin";
    public const string Analyst = "analyst";

    public static Dictionary<string, string> GetAllRoles()
    {
        return new Dictionary<string, string>
        {
            { nameof(SystemAdmin), SystemAdmin },
            { nameof(DataAdmin), DataAdmin },
            { nameof(Analyst), Analyst }
        };
    }
}