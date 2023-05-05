namespace Rinkudesu.Identity.Service.Utilities;

public static class EnvironmentalVariablesReader
{
    public const string DbContextVariableName = "RINKU_IDENTITY_CONNECTIONSTRING";
    public const string BaseUrlVariableName = "RINKU_IDENTITY_BASEURL";
    public const string RedisAddressVariableName = "RINKU_IDENTITY_REDIS";

    public static string GetRequiredVariable(string variableName)
    {
        var variableValue = Environment.GetEnvironmentVariable(variableName);
        if (variableValue is null)
        {
            throw new InvalidOperationException($"Environmental variable {variableName} is not set");
        }
        return variableValue;
    }
}
