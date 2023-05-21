using RInkudesu.Identity.Service.Common.Utilities;

namespace Rinkudesu.Identity.Service.Utilities;

public static class ArgonSettingsReader
{
    public static byte[] GetSecret() => System.Text.Encoding.UTF8.GetBytes(EnvironmentalVariablesReader.GetRequiredVariable("RINKU_ARGON_SECRET"));
}
