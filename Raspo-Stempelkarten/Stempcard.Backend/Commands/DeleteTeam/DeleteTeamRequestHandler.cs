using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteTeam;

[UsedImplicitly]
public class DeleteTeamRequestHandler(KurrentDBClient kurrentDbClient) 
    : IRequestHandler<DeleteTeamRequest, Task<Result>>
{
    public async Task<Result> Handle(DeleteTeamRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await kurrentDbClient.DeleteAsync(
                request.Id,
                StreamState.StreamExists,
                cancellationToken: cancellationToken);
            return Result.Ok();
        }
        catch (WrongExpectedVersionException)
        {
            return Result.Fail("Team doesn't exist!");
        }
    }
}