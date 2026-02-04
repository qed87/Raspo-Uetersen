using System.Security.Claims;
using DispatchR;
using JetBrains.Annotations;
using KurrentDB.Client;
using StampCard.Backend.Exceptions;
using StampCard.Backend.Model;

namespace StampCard.Backend.Services;

/// <summary>
/// The team model loader.
/// </summary>
/// <param name="mediator">The mediator.</param>
/// <param name="kurrentDbClient">The kurrent db client.</param>
[UsedImplicitly]
public class TeamModelLoader(
    IMediator mediator,
    IHttpContextAccessor httpContextAccessor,
    KurrentDBClient kurrentDbClient,
    ILoggerFactory loggerFactory)
    : ITeamModelLoader
{
    /// <inheritdoc />
    public async Task<ITeamAggregate?> LoadModelAsync(string streamId, ulong? expectedVersion = null, long? position = null)
    {
        var logger = loggerFactory.CreateLogger<TeamModelLoader>();
        var startFromPosition = position is not null 
            ? StreamPosition.FromInt64(position.Value) 
            : StreamPosition.Start;
        logger.LogDebug("Reading repository '{StreamId}' from '{StreamPosition}'.", streamId, startFromPosition.ToUInt64());
        var result = kurrentDbClient.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            startFromPosition);
        var team = new Team(mediator, httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal());
        var replayer = new TeamModelReplayer(loggerFactory.CreateLogger<TeamModelReplayer>(), team);
        StreamPosition? streamRevision = null;
        if (await result.ReadState == ReadState.Ok)
        {
            await foreach (var resolvedEvent in result)
            {
                replayer.Replay(resolvedEvent);
                streamRevision = resolvedEvent.OriginalEventNumber;
            }
        }
        else
        {
            return null;
        }

        if (expectedVersion is not null && (streamRevision is null || streamRevision.GetValueOrDefault().ToUInt64() != expectedVersion))
        {
            logger.LogInformation("Expected version doesn't match loaded version: {ExpectedVersion} != {StreamRevision}.", 
                expectedVersion, streamRevision?.ToUInt64());
            throw new ModelConcurrencyException();
        }

        logger.LogTrace("Retrieve model aggregate...");
        var modelAggregate = replayer.GetModel();
        modelAggregate.Id = streamId;
        modelAggregate.Version = streamRevision?.ToUInt64();
        return modelAggregate;
    }
}