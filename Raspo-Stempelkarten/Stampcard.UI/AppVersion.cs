using System.Reflection;

namespace Stampcard.UI;

/// <summary>
/// The version of the application, which the CI determines from the git tags, e.g. 1.2.0+a1b2c3d.
/// Local builds carry the full commit hash, it is shortened like the one of the CI.
/// </summary>
public static class AppVersion
{
    public static readonly string Current = Determine();

    private static string Determine()
    {
        var version = typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        var metadataStart = version.IndexOf('+');
        return metadataStart >= 0 && version.Length > metadataStart + 8 ? version[..(metadataStart + 8)] : version;
    }
}
