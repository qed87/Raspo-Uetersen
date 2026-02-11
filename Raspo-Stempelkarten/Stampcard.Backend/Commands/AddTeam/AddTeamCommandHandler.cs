using System.Text.Json;
using DispatchR.Abstractions.Send;
using FluentResults;
using FluentValidation;
using JetBrains.Annotations;
using KurrentDB.Client;
using StampCard.Backend.Events;
using StampCard.Backend.Services;
using StampCard.Backend.Services.Interfaces;

namespace StampCard.Backend.Commands.AddTeam;

/// <inheritdoc />
[UsedImplicitly]
public class AddTeamCommandHandler(
    KurrentDBClient kurrentDbClient,
    IUserProvider userProvider,
    ITeamService teamService,
    ILogger<AddTeamCommandHandler> logger,
    IValidator<AddTeamCommand> validator) : IRequestHandler<AddTeamCommand, Task<Result<string>>>
{
    /// <inheritdoc />
    public async Task<Result<string>> Handle(AddTeamCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                logger.LogInformation("Error during command validation: {ValidationErrors}.", string.Join(",", validationResult.Errors.Select(e => e.ErrorMessage)));
                return Result.Fail(validationResult.Errors.Select(x => x.ErrorMessage));
            } 
            
            var teams = await teamService.ListTeamsAsync(cancellationToken) ?? [];
            if (teams.Exists(dto => dto.Name == command.Name))
            {
                return Result.Fail("Team mit dem gleichen Namen existiert bereits.");
            }

            var teamId = Guid.NewGuid();
            var streamId = $"team-{teamId:N}";
            logger.LogInformation("Add Team '{SteamId}' to database...", streamId);
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
            return Result.Ok(streamId);
        }
        catch (WrongExpectedVersionException)
        {
            return Result.Fail<string>("Repository existiert bereits!");
        }
    }
}