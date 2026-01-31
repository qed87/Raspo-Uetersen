using System.Text.Json;
using DispatchR.Abstractions.Send;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.AddTeam;

/// <inheritdoc />
[UsedImplicitly]
public class AddTeamRequestHandler(
    KurrentDBClient kurrentDbClient, 
    ITeamService teamService,
    IValidator<AddTeamRequest> validator) : IRequestHandler<AddTeamRequest, Task<Result<Guid>>>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddTeamRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid) return Result.Fail(validationResult.Errors.Select(x => x.ErrorMessage)); 
            var teams = await teamService.ListTeamsAsync(cancellationToken) ?? [];
            if (teams.Exists(dto => dto.Name == request.Name))
            {
                return Result.Fail("Team mit dem gleichen Namen existiert bereits.");
            }

            var teamId = Guid.NewGuid();
            var streamId = $"team-{teamId:N}";
            await kurrentDbClient.AppendToStreamAsync(
                streamId,
                StreamState.NoStream,
                [
                    new EventData(
                        Uuid.NewUuid(),
                        nameof(TeamAdded),
                        JsonSerializer.SerializeToUtf8Bytes(
                            new TeamAdded(
                                request.Club, 
                                request.Name, 
                                request.IssuedBy, 
                                request.IssuedDate)))
                ],
                cancellationToken: cancellationToken);
            return Result.Ok(teamId);
        }
        catch (WrongExpectedVersionException)
        {
            return Result.Fail<Guid>("Repository existiert bereits!");
        }
    }
}