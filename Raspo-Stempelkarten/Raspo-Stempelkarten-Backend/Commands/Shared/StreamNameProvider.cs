using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

[UsedImplicitly]
public partial class StreamNameProvider : IStreamNameProvider
{
    [GeneratedRegex(@"[\s/-]+")]
    private static partial Regex SpecialCharRegex();
    
    public string GetStreamName(string team, string season)
    {
        return $"StampCard-{SpecialCharRegex().Replace(team, "_")}-{SpecialCharRegex().Replace(season, "_")}";
    }
}