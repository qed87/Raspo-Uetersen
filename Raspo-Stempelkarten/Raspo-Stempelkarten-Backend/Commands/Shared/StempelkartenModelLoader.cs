using System.Text.RegularExpressions;
using FluentValidation;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

internal partial class StempelkartenModelLoader(KurrentDBClient kurrentDbClient, IValidator<StempelkartenAggregate> validator) : IStempelkartenModelLoader
{
    [GeneratedRegex(@"[\s/]+")]
    public static partial Regex SpecialCharRegex();
    
    public async Task<StempelkartenAggregate> LoadModelAsync(
        string team, string season)
    {
        var stempelkarten = new StempelkartenAggregate
        {
            Team = team,
            Season = season
        };
        
        var result = kurrentDbClient.ReadStreamAsync(
            Direction.Forwards,
            $"Stempelkarten-{SpecialCharRegex().Replace(team, "_")}-{SpecialCharRegex().Replace(season, "_")}",
            StreamPosition.Start);
        ulong? streamRevision = null;
        if (await result.ReadState == ReadState.Ok)
        {
            await foreach (var resolvedEvent in result)
            {
                stempelkarten.Replay(resolvedEvent);
                streamRevision = resolvedEvent.OriginalEventNumber.ToUInt64();
            }
        }
        
        var validationResult = await validator.ValidateAsync(stempelkarten);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        
        stempelkarten.SetLoaded(streamRevision);
        return stempelkarten;
    }
}