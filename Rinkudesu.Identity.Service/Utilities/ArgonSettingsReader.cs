using Rinkudesu.Identity.Service.Common.Utilities;
#pragma warning disable CS1591

namespace Rinkudesu.Identity.Service.Utilities;

public static class ArgonSettingsReader
{
    public static byte[] GetSecret() => System.Text.Encoding.UTF8.GetBytes(EnvironmentalVariablesReader.GetRequiredVariable("RINKU_ARGON_SECRET"));
}
