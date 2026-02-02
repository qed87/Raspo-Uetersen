using System.Text.Json;
using DispatchR.Abstractions.Send;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using KurrentDB.Client;
using Microsoft.AspNetCore.SignalR;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.AddTeam;

/// <inheritdoc />
[UsedImplicitly]
public class AddTeamCommandHandler(
    KurrentDBClient kurrentDbClient,
    IUserProvider userProvider,
    ITeamService teamService,
    IValidator<AddTeamCommand> validator) : IRequestHandler<AddTeamCommand, Task<Result<Guid>>>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddTeamCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid) return Result.Fail(validationResult.Errors.Select(x => x.ErrorMessage)); 
            var teams = await teamService.ListTeamsAsync(cancellationToken) ?? [];
            if (teams.Exists(dto => dto.Name == command.Name))
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
                                command.Club, 
                                command.Name, 
                                userProvider.GetUserName() ?? string.Empty, 
                                DateTimeOffset.UtcNow)))
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