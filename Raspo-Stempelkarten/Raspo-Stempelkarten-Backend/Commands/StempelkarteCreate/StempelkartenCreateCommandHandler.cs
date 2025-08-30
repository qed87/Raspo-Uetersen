using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using LiteBus.Commands.Abstractions;
using Polly;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteCreate;

[UsedImplicitly]
public class StempelkartenCreateCommandHandler(
    KurrentDBClient kurrentDbClient, 
    IHttpContextAccessor contextAccessor, 
    IStempelkartenModelLoader modelLoader) 
    : ICommandHandler<StempelkartenCreateCommand, Result<StempelkartenCreateResponse>>
{

    public async Task<Result<StempelkartenCreateResponse>> HandleAsync(
        StempelkartenCreateCommand message, 
        CancellationToken cancellationToken = default)
    {
        var model = await modelLoader.LoadModelAsync(
            message.Dto.Team, message.Dto.Season);
        var result = model.AddStempelkarte(
            message.Dto.Recipient, 
            contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo",
            message.Dto.AdditionalOwner, 
            message.Dto.MinStamps, 
            message.Dto.MaxStamps);

        if (result.IsFailed)
        {
            return Result.Fail("Stempelkarte konnte nicht angelegt werden!");
        }
        
        if (!model.GetChanges().Any())
        {
            return Result.Ok();
        }
        
        IWriteResult writeResult;
        try
        {
            writeResult = await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(250 * i))
                .ExecuteAsync(async () => await kurrentDbClient.AppendToStreamAsync(
                    $"Stempelkarten-{StempelkartenModelLoader.SpecialCharRegex().Replace(message.Dto.Team, "_")}-{StempelkartenModelLoader.SpecialCharRegex().Replace(message.Dto.Season, "_")}",
                    model.StreamRevision is null
                        ? StreamState.NoStream
                        : StreamState.StreamRevision(model.StreamRevision.Value),
                    model.GetChanges(),
                    cancellationToken: cancellationToken));
        }
        catch (Exception ex)
        {
            return Result.Fail("Stempelkarte konnte nicht erstellt werden.");
        }

        return Result.Ok(new StempelkartenCreateResponse(
            result.Value.Id, 
            writeResult.NextExpectedStreamState.ToInt64()));
    }
}