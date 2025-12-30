using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

[UsedImplicitly]
public partial class StreamNameProvider : IStreamNameProvider
{
    [GeneratedRegex(@"[\s/-]+")]
    private static partial Regex SpecialCharRegex();
    
    public string GetStreamName(string season, string team)
    {
        return $"StampCard-{SpecialCharRegex().Replace(season, "_")}-{SpecialCharRegex().Replace(team, "_")}";
    }
}