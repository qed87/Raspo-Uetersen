using System.Text.Json;
using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Commands.AddTeam;

[UsedImplicitly]
public class AddTeamRequestHandler(KurrentDBClient kurrentDbClient) : IRequestHandler<AddTeamRequest, Task<Result<AddTeamResponse>>>
{
    public async Task<Result<AddTeamResponse>> Handle(AddTeamRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var streamId = $"{request.Club}-{request.BirthCohort:D4}";
            await kurrentDbClient.AppendToStreamAsync(
                streamId,
                StreamState.NoStream,
                [
                    new EventData(
                        Uuid.NewUuid(),
                        nameof(TeamAdded),
                        JsonSerializer.SerializeToUtf8Bytes(new TeamAdded(request.Club, request.BirthCohort))),
                    new EventData(
                        Uuid.NewUuid(),
                        nameof(CoachAdded),
                        JsonSerializer.SerializeToUtf8Bytes(new CoachAdded(request.Club, request.BirthCohort)))
                ],
                cancellationToken: cancellationToken);
            return Result.Ok(new AddTeamResponse(streamId));
        }
        catch (WrongExpectedVersionException)
        {
            return Result.Fail<AddTeamResponse>("Team already exists!");
        }
    }
}