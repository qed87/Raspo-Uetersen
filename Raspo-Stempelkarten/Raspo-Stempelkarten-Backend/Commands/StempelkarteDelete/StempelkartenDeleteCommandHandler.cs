using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using LiteBus.Commands.Abstractions;
using Polly;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteDelete;

[UsedImplicitly]
public class StempelkartenDeleteCommandHandler(
    KurrentDBClient kurrentDbClient, 
    IHttpContextAccessor contextAccessor, 
    IStempelkartenModelLoader modelLoader) 
    : ICommandHandler<StempelkartenDeleteCommand, Result>
{

    public async Task<Result> HandleAsync(
        StempelkartenDeleteCommand message, 
        CancellationToken cancellationToken = default)
    {
        var model = await modelLoader.LoadModelAsync(
            message.Team, message.Season);
        model.RemoveStempelkarte(
            message.Id,
            contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo");
        
        if (!model.GetChanges().Any())
        {
            return Result.Fail("Stempelkarte konnte nicht gelöscht werden!");
        }
        
        try
        {
            await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(250 * i))
                .ExecuteAsync(async () =>
                {
                    await kurrentDbClient.AppendToStreamAsync(
                        $"Stempelkarten-{StempelkartenModelLoader.SpecialCharRegex().Replace(message.Team, "_")}-{StempelkartenModelLoader.SpecialCharRegex().Replace(message.Season, "_")}",
                        model.StreamRevision is null
                            ? StreamState.NoStream
                            : StreamState.StreamRevision(model.StreamRevision.Value),
                        model.GetChanges(),
                        cancellationToken: cancellationToken);
                });
        }
        catch (Exception ex)
        {
            return Result.Fail("Stempelkarte konnte nicht gelöscht werden.");
        }

        return Result.Ok();
    }
}